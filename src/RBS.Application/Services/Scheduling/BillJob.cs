using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.Services.Billing;
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
                var periodParts = targetMonth.Split('-');
                var pYear = int.Parse(periodParts[0]);
                var pMonth = int.Parse(periodParts[1]);
                var lastDay = DateTime.DaysInMonth(pYear, pMonth);
                var dueDay = contract.EndDate != null ? Math.Min(contract.EndDate.Value.Day, lastDay) : lastDay;
                var dueDate = new DateOnly(pYear, pMonth, dueDay);

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
                    subjects.GetValueOrDefault("1122", Guid.Empty), ChinaTime.Now);

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

                    // GL入口
                    if (journal.Amount > 0 && subjects.ContainsKey("1122") && subjects.ContainsKey("6001"))
                    {
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                                CNo = contract.ContractNo ?? "", Period = targetMonth,
                                SId = subjects["1122"], SCode = "1122", Dir = "Debit",
                                Amt = journal.Amount, SrcType = "BillJob", SrcId = journal.Id,
                                Desc = "", CBy = Guid.Empty }, tx);
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                                CNo = contract.ContractNo ?? "", Period = targetMonth,
                                SId = subjects["6001"], SCode = "6001", Dir = "Credit",
                                Amt = journal.Amount, SrcType = "BillJob", SrcId = journal.Id,
                                Desc = "", CBy = Guid.Empty }, tx);
                    }
                }

                // 拾取历史未入账单Journal GLPosted=0自动过账
                var unbilled = (await conn.QueryAsync<dynamic>(
                    _sql.Get("Billing.Select.Journal.UnbilledByContract"),
                    new { CId = contract.Id }, tx)).ToList();

                foreach (var ub in unbilled)
                {
                    var ubAmt = (decimal)ub.Amount;
                    totalAmount += ubAmt;

                    if ((bool)ub.GLPosted == false && ubAmt > 0 && subjects.ContainsKey("1122") && subjects.ContainsKey("6001"))
                    {
                        await conn.ExecuteAsync(_sql.Get("Billing.Update.Journal.Post"),
                            new { Id = (Guid)ub.Id }, tx);
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                                CNo = contract.ContractNo ?? "", Period = targetMonth,
                                SId = subjects["1122"], SCode = "1122", Dir = "Debit",
                                Amt = ubAmt, SrcType = "BillJob", SrcId = (Guid)ub.Id,
                                Desc = "", CBy = Guid.Empty }, tx);
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                                CNo = contract.ContractNo ?? "", Period = targetMonth,
                                SId = subjects["6001"], SCode = "6001", Dir = "Credit",
                                Amt = ubAmt, SrcType = "BillJob", SrcId = (Guid)ub.Id,
                                Desc = "", CBy = Guid.Empty }, tx);
                    }
                }

                await _stepLogger.CompleteStepAsync(step03, created, null, token);

                if (created > 0 || feeConfigs.Count > 0)
                {
                    var step04 = await _stepLogger.StartStepAsync(taskLogId, "BillStep04",
                        $"生成账单-{contract.ContractNo}", null, null, token);

                    // 查询合同预存余额
                    var prepaid = contract.PrepaidBalance;
                    var dnId = Guid.NewGuid();
                    await PersistDebitNoteAsync(conn, dnId, contract.Id, contract.ContractNo ?? "",
                        companyId, targetMonth, totalAmount, prepaid, taskLogId, false, null, tx, token);

                    // 回写 DNId 到所有 Journals
                    foreach (var j in plans)
                    {
                        if (j.Amount > 0)
                            await conn.ExecuteAsync(_sql.Get("Billing.Update.Journal.SetDebitNoteId"),
                                new { DNId = dnId, Id = j.Id }, tx);
                    }
                    foreach (var ub in unbilled)
                    {
                        await conn.ExecuteAsync(_sql.Get("Billing.Update.Journal.SetDebitNoteId"),
                            new { DNId = dnId, Id = (Guid)ub.Id }, tx);
                    }

                    await _stepLogger.CompleteStepAsync(step04, 1, null, token);
                }

                // 更新合同欠款余额
                if (created > 0)
                {
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                        new { Id = contract.Id, Amt = totalAmount }, tx);
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

    private async Task PersistDebitNoteAsync(IDbConnection conn, Guid dnId, Guid contractId,
        string contractNo, Guid companyId, string period, decimal totalAmount, decimal prepaid,
        Guid taskLogId, bool isHistorical, DateOnly? dueDate, IDbTransaction tx, CancellationToken ct)
    {
        var periodParts = period.Split('-');
        var periodYear = int.Parse(periodParts[0]);
        var periodMonth = int.Parse(periodParts[1]);

        // 收集快照数据
        var tenantName = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.Tenant.PrimaryNameByContract"), new { Id = contractId }, tx);
        var buildingAddress = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.HousingUnit.BuildingAddressByContract"), new { Id = contractId }, tx);
        var companyRow = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Organization.Select.Company.ById"), new { Id = companyId }, tx);
        var companyName = companyRow?.Name as string ?? "";
        var previousBalance = await conn.QuerySingleOrDefaultAsync<decimal>(
            _sql.Get("Billing.Select.Journal.PreviousBalance"),
            new { ContractId = contractId, Year = periodYear, Month = periodMonth }, tx);

        var noteNo = await DebitNoteService.GenerateBillNoAsync(conn, tx);
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.DebitNote.FromBillJob"),
            new
            {
                Id = dnId, NoteNo = noteNo,
                CId = contractId, CoId = companyId, Amt = totalAmount,
                IsHist = isHistorical, Due = dueDate,
                PeriodYear = periodYear, PeriodMonth = periodMonth, CBy = Guid.Empty,
                ContractNo = contractNo,
                TenantName = tenantName ?? "",
                BuildingAddress = buildingAddress ?? "",
                CompanyName = companyName ?? "",
                PreviousBalance = previousBalance
            }, tx);
    }

    protected override async Task<string> BuildDryRunReportAsync(
        Guid companyId, string targetMonth, Guid taskLogId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var contracts = await _uow.Contracts.GetActiveContractsAsync(companyId, ct);
        var matched = contracts.Where(c => c.ShouldGenerateReceivableFor(targetMonth)).ToList();
        var periodParts = targetMonth.Split('-');
        var pYear = int.Parse(periodParts[0]);
        var pMonth = int.Parse(periodParts[1]);
        var pLastDay = DateTime.DaysInMonth(pYear, pMonth);
        int totalFees = 0;
        var warnings = new List<string>();
        foreach (var c in matched)
        {
            var fees = (await conn.QueryAsync(_sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                new { ContractId = c.Id, PeriodStart = $"{targetMonth}-01", PeriodEnd = $"{targetMonth}-{pLastDay}" })).ToList();
            totalFees += fees.Count;
            if (fees.Count == 0) warnings.Add($"合同 {c.ContractNo} 无生效费用配置");
        }
        var report = new { totalContracts = matched.Count, totalFeeConfigs = totalFees,
            estimatedReceivables = totalFees, estimatedBills = matched.Count, warnings };
        return JsonSerializer.Serialize(report);
    }
}
