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
        JobExecutionContext jobContext)
        : base(taskLogRepo, stepLogger, uow, jobContext)
    {
        _db = db;
        _sql = sql;
    }

    protected override async Task<JobResult> ExecuteCoreAsync(
        Guid companyId, string targetMonth, ExecuteMode mode, CancellationToken ct)
    {
        var taskLogId = await BeginTaskLogAsync(JobName, companyId, targetMonth,
            "Manual", mode == ExecuteMode.DryRun ? "DryRun" : "Execute", null, ct);

        if (mode == ExecuteMode.DryRun)
            return new JobResult(0, 0, summary: "DryRun 完成");

        // 预查会计科目
        var subjects = await LoadSubjectsAsync(ct);

        var step01 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep01", "查询待处理合同", null, null, ct);
        var contracts = await _uow.Contracts.GetActiveContractsAsync(companyId, ct);
        await _stepLogger.CompleteStepAsync(step01, contracts.Count, null, ct);

        if (contracts.Count == 0)
            return new JobResult(0, 0, summary: "无待处理合同");

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
                var today = DateOnly.FromDateTime(ChinaTime.Now);

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
                        var billedAt = DateTime.UtcNow;
                        // Journal: 预收转应收
                        await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                            new { Id = Guid.NewGuid(), CoId = companyId, CId = contract.Id,
                                FeeCodeId = Guid.Empty, FConfigId = (Guid?)null,
                                SubjId = subjects["2203"], Period = targetMonth,
                                Amt = amt, Due = today, EntryType = "Adjustment",
                                BilledAt = billedAt, DNId = (Guid?)null,
                                ParentId = (Guid?)null, Summary = "预收抵应收", CBy = Guid.Empty }, tx);
                        await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                            new { Id = Guid.NewGuid(), CoId = companyId, CId = contract.Id,
                                FeeCodeId = Guid.Empty, FConfigId = (Guid?)null,
                                SubjId = subjects["1122"], Period = targetMonth,
                                Amt = -amt, Due = today, EntryType = "Adjustment",
                                BilledAt = billedAt, DNId = (Guid?)null,
                                ParentId = (Guid?)null, Summary = "预收抵应收", CBy = Guid.Empty }, tx);
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

                // Step03: 滞纳金
                var step03 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep03",
                    $"滞纳金-{contract.ContractNo}", null, null, token);
                var overdueJournals = (await conn.QueryAsync<dynamic>(
                    _sql.Get("Billing.Select.Journal.OverdueByContract"),
                    new { CId = contract.Id, Today = today }, tx)).ToList();

                int penaltyCount = 0;
                foreach (var journal in overdueJournals)
                {
                    var daysOverdue = today.DayNumber - ((DateOnly)journal.DueDate).DayNumber;
                    if (daysOverdue <= 0) continue;
                    var balance = (decimal)journal.Amount;
                    if (balance <= 0) continue;
                    var penalty = Math.Round(balance * 0.0005m * Math.Min(daysOverdue, 90), 2);
                    if (penalty <= 0) continue;
                    // 滞纳金 Journal + GL 更新
                    var billedAt = DateTime.UtcNow;
                    await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                        new { Id = Guid.NewGuid(), CoId = companyId, CId = contract.Id,
                            FId = (Guid)journal.FeeCodeId, FConfigId = (Guid?)null,
                            SubjId = subjects["1122"], Period = targetMonth,
                            Amt = penalty, Due = today, EntryType = "LateFee",
                            BilledAt = billedAt, DNId = (Guid?)null,
                            ParentId = (Guid)journal.Id, Summary = "滞纳金", CBy = Guid.Empty }, tx);
                    }

                await _stepLogger.CompleteStepAsync(step03, penaltyCount, null, token);
            }
            catch { }
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
