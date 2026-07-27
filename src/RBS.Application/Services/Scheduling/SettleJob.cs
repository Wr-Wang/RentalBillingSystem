using System.Collections.Concurrent;
using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Scheduling;

public class SettleJob : ScheduledJobBase
{
    public override string JobName => "SettleJob";

    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public SettleJob(
        ITaskLogRepository taskLogRepo, ITaskStepLogger stepLogger, IUnitOfWork uow,
        IDbConnectionFactory db, ISqlLoader sql,
        JobExecutionContext jobContext,
        SchedulingOptions? options = null)
        : base(taskLogRepo, stepLogger, uow, jobContext, options)
    {
        _db = db;
        _sql = sql;
    }

    protected override async Task<JobResult> ExecuteCoreAsync(
        Guid companyId, string targetMonth, ExecuteMode mode, CancellationToken ct)
    {
        var taskLogId = await BeginTaskLogAsync(JobName, companyId, targetMonth,
            "Manual", mode == ExecuteMode.DryRun ? "DryRun" : "Execute", null, ct);

        // 预查会计科目
        var subjects = await LoadSubjectsAsync(ct);

        var step01 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep01", "查询待处理合同", null, null, ct);
        var contracts = await _uow.Contracts.GetActiveContractsAsync(companyId, ct);
        await _stepLogger.CompleteStepAsync(step01, contracts.Count, null, ct);

        if (contracts.Count == 0)
            return new JobResult(0, 0, summary: "无待处理合同");

        if (mode == ExecuteMode.DryRun)
        {
            var totalPrepaid = contracts.Sum(c => c.PrepaidBalance);
            var totalOutstanding = contracts.Sum(c => c.OutstandingBalance);
            return new JobResult(contracts.Count, 0, summary:
                $"DryRun：{contracts.Count} 份待处理合同，预存总额 {totalPrepaid:N2}，欠款总额 {totalOutstanding:N2}");
        }

        var counters = new int[3]; // [0]=offset, [1]=penalty, [2]=overdue
        var success = 0; var fail = 0;
        var errors = new ConcurrentBag<(Guid, string)>();

        await Parallel.ForEachAsync(contracts,
            new ParallelOptions { MaxDegreeOfParallelism = ContractParallelism, CancellationToken = ct },
            async (contract, token) =>
        {
            try
            {
                using var conn = _db.CreateConnection(); conn.Open();
                using var tx = conn.BeginTransaction();
                var today = ChinaTime.Now.Date;

                // Step02: 预收抵应收（基于合同独立预存金额字段）
                if (contract.PrepaidBalance <= 0)
                {
                    await _stepLogger.SkipStepAsync(
                        await _stepLogger.StartStepAsync(taskLogId, "SettleStep02",
                            $"预收抵应收-{contract.ContractNo}", null, null, token),
                        "无预存金额，跳过", null, token);
                }
                else
                {
                    var step02 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep02",
                        $"预收抵应收-{contract.ContractNo}", null, null, token);

                    var receivable = await conn.QuerySingleAsync<decimal>(
                        _sql.Get("Billing.Select.Journal.BalanceByContract"),
                        new { Code = "1122", CId = contract.Id }, tx);

                    if (receivable > 0)
                    {
                        var amt = Math.Min(contract.PrepaidBalance, receivable);
                        // 写 GL 分录（不生成 Journal）
                        if (subjects.ContainsKey("2203") && subjects.ContainsKey("1122"))
                        {
                            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                                new { Id = Guid.NewGuid(), CoId = companyId, CId = contract.Id,
                                    CNo = contract.ContractNo ?? "", Period = targetMonth,
                                    SId = subjects["2203"], SCode = "2203", Dir = "Debit",
                                    Amt = amt, SrcType = "SettleOffset", SrcId = (Guid?)null,
                                    Desc = "", CBy = Guid.Empty }, tx);
                            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                                new { Id = Guid.NewGuid(), CoId = companyId, CId = contract.Id,
                                    CNo = contract.ContractNo ?? "", Period = targetMonth,
                                    SId = subjects["1122"], SCode = "1122", Dir = "Credit",
                                    Amt = amt, SrcType = "SettleOffset", SrcId = (Guid?)null,
                                    Desc = "", CBy = Guid.Empty }, tx);
                        }
                        // 扣减合同预存金额 + 欠款余额
                        await conn.ExecuteAsync(
                            _sql.Get("Accounting.Update.Contract.PrepaidBalanceDecrement"),
                            new { Amt = amt, Id = contract.Id }, tx);
                        await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                            new { Id = contract.Id, Amt = -amt }, tx);
                        counters[0]++;
                    }
                    await _stepLogger.CompleteStepAsync(step02, counters[0], null, token);
                }

                // Step03: 利息
                var step03 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep03",
                    $"利息-{contract.ContractNo}", null, null, token);
                var overdueJournals = (await conn.QueryAsync<dynamic>(
                    _sql.Get("Billing.Select.Journal.OverdueByContract"),
                    new { CId = contract.Id, Today = today }, tx)).ToList();

                int interestCount = 0;
                foreach (var journal in overdueJournals)
                {
                    var daysOverdue = (today - ((DateTime)journal.DueDate).Date).Days;
                    if (daysOverdue <= 0) continue;
                    var balance = (decimal)journal.Amount;
                    if (balance <= 0) continue;
                    var interest = Math.Round(balance * 0.0005m * Math.Min(daysOverdue, 90), 2);
                    if (interest <= 0) continue;

                    // 幂等检查：已存在该逾期父单的利息分录则跳过
                    var exists = await conn.QuerySingleAsync<int>(
                        _sql.Get("Billing.Select.Journal.InterestExists"),
                        new { ParentId = (Guid)journal.Id }, tx);
                    if (exists > 0) continue;

                    var billedAt = ChinaTime.Now;
                    await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Interest"),
                        new { Id = Guid.NewGuid(), CoId = companyId, CId = contract.Id,
                            FId = (Guid)journal.FeeCodeId, FConfigId = (Guid?)null,
                            SubjId = subjects["1122"], Period = targetMonth,
                            Amt = interest, Due = today,
                            BilledAt = billedAt, DNId = (Guid?)null,
                            ParentId = (Guid)journal.Id, Summary = "", CBy = Guid.Empty }, tx);
                    // 同步更新合同欠款余额
                    await conn.ExecuteAsync(
                        _sql.Get("Billing.Update.Contract.OutstandingBalanceAddInterest"),
                        new { Amt = interest, Id = contract.Id }, tx);
                    interestCount++;
                    }

                await _stepLogger.CompleteStepAsync(step03, interestCount, null, token);

                tx.Commit();
                Interlocked.Increment(ref success);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref fail);
                errors.Add((contract.Id, ex.Message));
            }
        });

        var result = new JobResult(success, fail, errors,
            summary: $"{success}/{contracts.Count} 合同完成");
        await CompleteTaskLogAsync(taskLogId, result, ct);
        return result;
    }

    private async Task<Dictionary<string, Guid>> LoadSubjectsAsync(CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes")))
            .ToDictionary(r => r.Code, r => r.Id);
        return subjects;
    }
}
