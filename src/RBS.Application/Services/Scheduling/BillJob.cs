using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Scheduling;

public class BillJob : ScheduledJobBase
{
    public override string JobName => "BillJob";

    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IBillingDomainService _billingDomain;

    public BillJob(
        ITaskLogRepository taskLogRepo, ITaskStepLogger stepLogger, IUnitOfWork uow,
        IDbConnectionFactory db, ISqlLoader sql,
        IBillingDomainService billingDomain,
        JobExecutionContext jobContext)
        : base(taskLogRepo, stepLogger, uow, jobContext)
    {
        _db = db;
        _sql = sql;
        _billingDomain = billingDomain;
    }

    protected override async Task<JobResult> ExecuteCoreAsync(
        Guid companyId, string targetMonth, ExecuteMode mode, CancellationToken ct)
    {
        var taskLogId = await BeginTaskLogAsync(JobName, companyId, targetMonth,
            "Manual", mode == ExecuteMode.DryRun ? "DryRun" : "Execute", null, ct);

        if (mode == ExecuteMode.DryRun)
        {
            var report = await BuildDryRunReportAsync(companyId, targetMonth, taskLogId, ct);
            await SetDryRunResultAsync(taskLogId, report, ct);
            return new JobResult(0, 0, summary: "DryRun 完成");
        }

        // 预查会计科目（一次查询，后续复用）
        var subjects = await LoadAccountingSubjectsAsync(ct);

        // Step01
        var step01 = await _stepLogger.StartStepAsync(taskLogId, "BillStep01", "查询待处理合同", null, null, ct);
        var contracts = await _uow.Contracts.GetActiveContractsAsync(companyId, ct);
        var matched = contracts.Where(c => c.ShouldGenerateReceivableFor(targetMonth)).ToList();
        await _stepLogger.CompleteStepAsync(step01, matched.Count, null, ct);
        if (matched.Count == 0)
            return new JobResult(0, 0, summary: "无待处理合同");

        // Step02~05: 并行
        var success = 0; var fail = 0;
        var errors = new ConcurrentBag<(Guid, string)>();

        await Parallel.ForEachAsync(matched,
            new ParallelOptions { MaxDegreeOfParallelism = ContractParallelism, CancellationToken = ct },
            async (contract, token) =>
        {
            try
            {
                using var conn = _db.CreateConnection(); conn.Open();
                using var tx = conn.BeginTransaction();

                var step02 = await _stepLogger.StartStepAsync(taskLogId, "BillStep02",
                    $"加载费用-{contract.ContractNo}", null, null, token);
                var today = ChinaTime.Now;
                var lastDay = DateTime.DaysInMonth(today.Year, today.Month);
                var dueDay = contract.EndDate != null ? Math.Min(contract.EndDate.Value.Day, lastDay) : lastDay;
                var dueDate = new DateOnly(today.Year, today.Month, dueDay);

                var feeConfigs = (await conn.QueryAsync<(Guid FeeCodeId, decimal Amount, string? EffDate, string? ExpDate, string FeeName)>(
                    _sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                    new { ContractId = contract.Id, PeriodStart = $"{targetMonth}-01", PeriodEnd = $"{targetMonth}-{lastDay}" }, tx)).ToList();
                await _stepLogger.CompleteStepAsync(step02, feeConfigs.Count, null, token);

                var step03 = await _stepLogger.StartStepAsync(taskLogId, "BillStep03",
                    $"生成应收-{contract.ContractNo}", null, null, token);
                int created = 0;
                decimal totalAmount = 0;

                var plans = _billingDomain.GenerateProratedJournals(
                    feeConfigs.Select(f => (f.FeeCodeId, f.Amount, f.EffDate, f.ExpDate, f.FeeName)).ToList(),
                    contract.Id, targetMonth, dueDate, contract.CompanyId,
                    subjects.GetValueOrDefault("1122", Guid.Empty), DateTime.UtcNow);

                foreach (var journal in plans)
                {
                    var exists = await conn.QuerySingleAsync<int>(
                        _sql.Get("Billing.Select.Journal.ExistsByKey"),
                        new { C = contract.Id, F = journal.FeeCodeId, P = targetMonth }, tx);
                    if (exists > 0) continue;

                    await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                        new
                        {
                            Id = journal.Id, CoId = journal.CompanyId, CId = journal.ContractId,
                            FId = journal.FeeCodeId, FConfigId = journal.FeeConfigId,
                            SubjId = journal.AccountingSubjectId, Period = journal.Period,
                            Amt = journal.Amount, Due = journal.DueDate, EntryType = journal.EntryType,
                            BilledAt = journal.BilledAt, DNId = journal.DebitNoteId,
                            ParentId = journal.ParentJournalId, Summary = journal.Summary, CBy = Guid.Empty
                        }, tx);
                    created++;
                    if (journal.Amount > 0) totalAmount += journal.Amount;
                }

                // Step03b: 汇总未结清的 Supplementary Journal
                var suppEntries = (await conn.QueryAsync<(Guid JournalId, decimal Amount)>(
                    _sql.Get("Billing.Select.Journal.SupplementaryDue"),
                    new { Cid = contract.Id }, tx)).ToList();

                foreach (var (journalId, amt) in suppEntries)
                {
                    totalAmount += amt;
                    created++;
                }

                await _stepLogger.CompleteStepAsync(step03, created, null, token);

                if (created > 0 || feeConfigs.Count > 0)
                {
                    var step04 = await _stepLogger.StartStepAsync(taskLogId, "BillStep04",
                        $"生成账单-{contract.ContractNo}", null, null, token);
                    await PersistDebitNoteAsync(conn, contract.Id, contract.ContractNo ?? "",
                        companyId, targetMonth, totalAmount, taskLogId, false, null, tx, token);
                    await _stepLogger.CompleteStepAsync(step04, 1, null, token);
                }

                // 更新合同欠款余额
                if (created > 0)
                {
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                        new { Id = contract.Id, Amt = totalAmount }, tx);
                }

                // 写入 GL（追加式快照，按合同累加本期出账总额）
                if (created > 0)
                {
                    var glLatest = await conn.QuerySingleOrDefaultAsync(
                        _sql.Get("Accounting.Select.GL.LatestByPeriod"),
                        new { CoId = companyId, Period = targetMonth }, tx);
                    var prevBilled = glLatest != null ? (decimal)((dynamic)glLatest).TotalBilled : 0m;
                    var prevReceived = glLatest != null ? (decimal)((dynamic)glLatest).TotalReceived : 0m;
                    var opening = glLatest != null ? (decimal)((dynamic)glLatest).OpeningBalance
                        : await GetOpeningFromPrevPeriod(companyId, targetMonth, conn, tx);
                    var totalBilled = prevBilled + totalAmount;
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Default"),
                        new { Id = Guid.NewGuid(), CoId = companyId, Period = targetMonth,
                            Opening = opening, Billed = totalBilled, Received = prevReceived,
                            Closing = opening + totalBilled - prevReceived, CBy = Guid.Empty }, tx);
                }

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
            summary: $"{success}/{matched.Count} 合同完成");
        await CompleteTaskLogAsync(taskLogId, result, ct);
        return result;
    }

    private async Task<Dictionary<string, Guid>> LoadAccountingSubjectsAsync(CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes"));
        return rows.ToDictionary(r => r.Code, r => r.Id);
    }

    private async Task PersistDebitNoteAsync(IDbConnection conn, Guid contractId,
        string contractNo, Guid companyId, string period, decimal totalAmount,
        Guid taskLogId, bool isHistorical, DateOnly? dueDate, IDbTransaction tx, CancellationToken ct)
    {
        var roomCode = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.HousingUnit.RoomCodeByContract"), new { Id = contractId }, tx);
        var tenantName = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.Tenant.PrimaryNameByContract"), new { Id = contractId }, tx);

        await conn.ExecuteAsync(_sql.Get("Lease.Insert.DebitNote.FromBillJob"),
            new
            {
                Id = Guid.NewGuid(), NoteNo = $"DN-{contractNo}-{period.Replace("-", "")}",
                CId = contractId, CNo = contractNo ?? "", Period = period, CoId = companyId,
                Room = roomCode ?? "", Tenant = tenantName ?? "", Amt = totalAmount,
                IsHist = isHistorical, Due = dueDate,
                TaskLogId = taskLogId, CBy = Guid.Empty
            }, tx);
    }

    protected override async Task<string> BuildDryRunReportAsync(
        Guid companyId, string targetMonth, Guid taskLogId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var contracts = await _uow.Contracts.GetActiveContractsAsync(companyId, ct);
        var matched = contracts.Where(c => c.ShouldGenerateReceivableFor(targetMonth)).ToList();
        int totalFees = 0;
        var warnings = new List<string>();
        foreach (var c in matched)
        {
            var fees = (await conn.QueryAsync(_sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                new { ContractId = c.Id, PeriodStart = $"{targetMonth}-01", PeriodEnd = $"{targetMonth}-{DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)}" })).ToList();
            totalFees += fees.Count;
            if (fees.Count == 0) warnings.Add($"合同 {c.ContractNo} 无生效费用配置");
        }
        var report = new { totalContracts = matched.Count, totalFeeConfigs = totalFees,
            estimatedReceivables = totalFees, estimatedBills = matched.Count, warnings };
        return JsonSerializer.Serialize(report);
    }

    private async Task<decimal> GetOpeningFromPrevPeriod(Guid companyId, string period, IDbConnection conn, IDbTransaction? tx)
    {
        var parts = period.Split('-');
        var py = int.Parse(parts[0]); var pm = int.Parse(parts[1]) - 1;
        if (pm == 0) { py--; pm = 12; }
        var prev = await conn.QuerySingleOrDefaultAsync(
            _sql.Get("Accounting.Select.GL.OpeningBalanceByPeriod"),
            new { CoId = companyId, Period = $"{py:D4}-{pm:D2}" }, tx);
        return prev != null ? (decimal)((dynamic)prev).ClosingBalance : 0m;
    }
}
