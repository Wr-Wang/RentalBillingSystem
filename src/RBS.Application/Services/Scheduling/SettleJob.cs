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
        IDbConnectionFactory db, ISqlLoader sql)
        : base(taskLogRepo, stepLogger, uow)
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

                // Step02: 预收抵应收
                var step02 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep02",
                    $"预收抵应收-{contract.ContractNo}", null, null, token);

                var prepaid = await conn.QuerySingleAsync<decimal>(
                    _sql.Get("Billing.Select.JournalEntry.BalanceBySubject"),
                    new { Code = "2203", SrcId = contract.Id }, tx);
                var receivable = await conn.QuerySingleAsync<decimal>(
                    _sql.Get("Billing.Select.JournalEntry.BalanceBySubject"),
                    new { Code = "1122", SrcId = contract.Id }, tx);

                if (prepaid > 0 && receivable > 0)
                {
                    var amt = Math.Min(prepaid, receivable);
                    var vid = Guid.NewGuid();
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.Voucher.BillJob"),
                        new { Id = vid, No = $"STL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 32),
                            Date = today, Desc = $"SettleJob {targetMonth} 预收抵应收",
                            SrcId = contract.Id, Type = "SettleJob", CBy = Guid.Empty }, tx);
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = vid, SId = subjects["2203"],
                            Dir = "Debit", Amt = amt, Sum = "预收抵应收", CBy = Guid.Empty }, tx);
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = vid, SId = subjects["1122"],
                            Dir = "Credit", Amt = amt, Sum = "预收抵应收", CBy = Guid.Empty }, tx);
                    counters[0]++;
                }
                await _stepLogger.CompleteStepAsync(step02, counters[0], null, token);

                // Step03: 滞纳金
                var step03 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep03",
                    $"滞纳金-{contract.ContractNo}", null, null, token);
                var overduePlans = (await conn.QueryAsync<dynamic>(
                    _sql.Get("Billing.Select.ReceivablePlan.OverdueByContract"),
                    new { CId = contract.Id, Today = today }, tx)).ToList();

                int penaltyCount = 0;
                foreach (var plan in overduePlans)
                {
                    var daysOverdue = today.DayNumber - ((DateOnly)plan.DueDate).DayNumber;
                    if (daysOverdue <= 0) continue;
                    var balance = (decimal)plan.Amount - (decimal)plan.Received - (decimal)plan.LateFee;
                    if (balance <= 0) continue;
                    var penalty = Math.Round(balance * 0.0005m * Math.Min(daysOverdue, 90), 2);
                    if (penalty <= 0) continue;
                    await conn.ExecuteAsync(_sql.Get("Billing.Update.ReceivablePlan.LateFeeIncrement"),
                        new { Fee = penalty, Id = (Guid)plan.Id }, tx);
                    // 分录：借应收账款(1122) / 贷其他业务收入(6051)
                    var pvId = Guid.NewGuid();
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.Voucher.BillJob"),
                        new { Id = pvId, No = $"PEN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 32),
                            Date = today, Desc = $"SettleJob {targetMonth} 滞纳金",
                            SrcId = contract.Id, Type = "SettleJob", CBy = Guid.Empty }, tx);
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = pvId, SId = subjects["1122"],
                            Dir = "Debit", Amt = penalty, Sum = "滞纳金", CBy = Guid.Empty }, tx);
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = pvId, SId = subjects["6051"],
                            Dir = "Credit", Amt = penalty, Sum = "滞纳金", CBy = Guid.Empty }, tx);
                    penaltyCount++;
                }
                counters[1] += penaltyCount;
                await _stepLogger.CompleteStepAsync(step03, penaltyCount, null, token);

                // Step04: 逾期标记
                var step04 = await _stepLogger.StartStepAsync(taskLogId, "SettleStep04",
                    $"逾期标记-{contract.ContractNo}", null, null, token);
                var overdueCount = await conn.ExecuteAsync(
                    _sql.Get("Billing.Update.ReceivablePlan.MarkOverdueByContract"),
                    new { CId = contract.Id, Today = today }, tx);
                counters[2] += overdueCount;
                await _stepLogger.CompleteStepAsync(step04, overdueCount, null, token);

                tx.Commit();
                Interlocked.Increment(ref success);
            }
            catch (Exception ex)
            {
                errors.Add((contract.Id, ex.Message));
                Interlocked.Increment(ref fail);
            }
        });

        var result = new JobResult(success, fail, errors,
            summary: $"{success}/{contracts.Count} 完成，预收抵{counters[0]}，滞纳金{counters[1]}，逾期{counters[2]}");
        await CompleteTaskLogAsync(taskLogId, result, ct);
        return result;
    }

    private async Task<Dictionary<string, Guid>> LoadSubjectsAsync(CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<(string Code, Guid Id)>(
            "SELECT Code, Id FROM AccountingSubjects WHERE Code IN ('1122','2203','6051')");
        return rows.ToDictionary(r => r.Code, r => r.Id);
    }
}
