using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.Services.Billing;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Scheduling;
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
    private readonly IBillJobFailedContractRepository _failedContractRepo;
    private readonly IDebitNoteService _debitNoteService;
    private readonly int _pdfParallelism;

    public BillJob(
        ITaskLogRepository taskLogRepo, ITaskStepLogger stepLogger, IUnitOfWork uow,
        IDbConnectionFactory db, ISqlLoader sql,
        IBillingDomainService billingDomain,
        IBillJobFailedContractRepository failedContractRepo,
        IDebitNoteService debitNoteService,
        JobExecutionContext jobContext,
        SchedulingOptions? options = null)
        : base(taskLogRepo, stepLogger, uow, jobContext, options)
    {
        _db = db;
        _sql = sql;
        _billingDomain = billingDomain;
        _failedContractRepo = failedContractRepo;
        _debitNoteService = debitNoteService;
        _pdfParallelism = options?.PdfParallelism ?? 16;
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

        // Step02~05: 并行（每个合同独立事务，内部批量写入）
        var success = 0; var fail = 0;
        var errors = new ConcurrentBag<(Guid, string)>();
        var failedContracts = new ConcurrentBag<BillJobFailedContract>();
        var debitNoteIds = new ConcurrentBag<(Guid DnId, Guid CompanyId, int Year, int Month)>();

        await Parallel.ForEachAsync(matched,
            new ParallelOptions { MaxDegreeOfParallelism = ContractParallelism, CancellationToken = ct },
            async (contract, token) =>
        {
            string currentStep = "BillStep02";
            try
            {
                string? contractNo = contract.ContractNo;
                using var conn = _db.CreateConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                // ===== Step02: 加载费用配置 =====
                var step02 = await _stepLogger.StartStepAsync(taskLogId, "BillStep02",
                    $"加载费用-{contractNo}", null, null, token);
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

                // ===== Step03: 生成 Journals + GL + DebitNoteItems（批量收集→批量写入） =====
                currentStep = "BillStep03";
                var step03 = await _stepLogger.StartStepAsync(taskLogId, "BillStep03",
                    $"生成应收-{contractNo}", null, null, token);

                // 生成 prorated journals（内存计算）
                var plans = _billingDomain.GenerateProratedJournals(
                    feeConfigs.Select(f => (f.FeeCodeId, f.Amount, f.EffDate, f.ExpDate, f.FeeName)).ToList(),
                    contract.Id, targetMonth, dueDate, contract.CompanyId,
                    subjects.GetValueOrDefault("1122", Guid.Empty), ChinaTime.Now);

                // 批量查询本合同+本账期已存在的 FeeCodeId，用于去重
                var existingFeeCodes = (await conn.QueryAsync<Guid>(
                    _sql.Get("Billing.Select.Journal.ExistingFeeCodesByContractPeriod"),
                    new { ContractId = contract.Id, Period = targetMonth }, tx)).ToHashSet();

                // 收集待写入数据（跳过已存在的费用项目）
                var journalBatch = new List<object>(plans.Count);
                var glBatch = new List<object>(plans.Count * 2);
                var itemTuples = new List<(Guid FeeCodeId, decimal Amount, string FeeName)>(plans.Count);
                decimal totalAmount = 0;
                int created = 0;

                foreach (var journal in plans)
                {
                    if (journal.Amount <= 0) continue;
                    if (existingFeeCodes.Contains(journal.FeeCodeId)) continue;

                    // Journal
                    journalBatch.Add(new
                    {
                        Id = journal.Id, CoId = journal.CompanyId, CId = journal.ContractId,
                        FId = journal.FeeCodeId, FConfigId = journal.FeeConfigId,
                        SubjId = journal.AccountingSubjectId, Period = targetMonth,
                        Amt = journal.Amount, Due = journal.DueDate, EntryType = journal.EntryType,
                        BilledAt = journal.BilledAt, DNId = journal.DebitNoteId,
                        ParentId = journal.ParentJournalId, Summary = journal.Summary, CBy = Guid.Empty
                    });
                    totalAmount += journal.Amount;
                    created++;

                    // 暂存 DebitNoteItem 数据（DnId 稍后确定）
                    var feeName = feeConfigs.FirstOrDefault(f => f.FeeCodeId == journal.FeeCodeId).FeeName ?? "";
                    itemTuples.Add((journal.FeeCodeId, journal.Amount, feeName));

                    // GL 入口（借记 1122 + 贷记 6001）
                    if (subjects.ContainsKey("1122") && subjects.ContainsKey("6001"))
                    {
                        glBatch.Add(new
                        {
                            Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                            CNo = contractNo ?? "", Period = targetMonth,
                            SId = subjects["1122"], SCode = "1122", Dir = "Debit",
                            Amt = journal.Amount, SrcType = "BillJob", SrcId = journal.Id,
                            Desc = "", CBy = Guid.Empty
                        });
                        glBatch.Add(new
                        {
                            Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                            CNo = contractNo ?? "", Period = targetMonth,
                            SId = subjects["6001"], SCode = "6001", Dir = "Credit",
                            Amt = journal.Amount, SrcType = "BillJob", SrcId = journal.Id,
                            Desc = "", CBy = Guid.Empty
                        });
                    }
                }

                // 批量写入 Journals
                if (journalBatch.Count > 0)
                {
                    await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"), journalBatch, tx);
                }

                // 批量写入 GL Entries
                if (glBatch.Count > 0)
                {
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"), glBatch, tx);
                }

                // 处理历史未入账 Journals（GLPosted=0 自动过账）
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
                        // 未入账的 GL 只有 2 条，不批量
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                                CNo = contractNo ?? "", Period = targetMonth,
                                SId = subjects["1122"], SCode = "1122", Dir = "Debit",
                                Amt = ubAmt, SrcType = "BillJob", SrcId = (Guid)ub.Id,
                                Desc = "", CBy = Guid.Empty }, tx);
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contract.Id,
                                CNo = contractNo ?? "", Period = targetMonth,
                                SId = subjects["6001"], SCode = "6001", Dir = "Credit",
                                Amt = ubAmt, SrcType = "BillJob", SrcId = (Guid)ub.Id,
                                Desc = "", CBy = Guid.Empty }, tx);
                    }
                }

                await _stepLogger.CompleteStepAsync(step03, created, null, token);

                // ===== Step04: 生成 DebitNote + DebitNoteItems 快照 =====
                var dnId = Guid.Empty;
                if (created > 0 || feeConfigs.Count > 0)
                {
                    currentStep = "BillStep04";
                    var step04 = await _stepLogger.StartStepAsync(taskLogId, "BillStep04",
                        $"生成账单-{contractNo}", null, null, token);

                    var prepaid = contract.PrepaidBalance;
                    dnId = Guid.NewGuid();
                    await PersistDebitNoteAsync(conn, tx, new DebitNoteRequest(
                        dnId, contract.Id, contractNo ?? "", companyId,
                        targetMonth, totalAmount, taskLogId), token);

                    // 批量写入 DebitNoteItems（此时已拿到 dnId）
                    if (itemTuples.Count > 0)
                    {
                        var itemBatch = itemTuples.Select(t => new
                        {
                            Id = Guid.NewGuid(), DebitNoteId = dnId,
                            FeeCodeId = t.FeeCodeId, FeeName = t.FeeName,
                            Amount = t.Amount, CreatedBy = Guid.Empty,
                            CreatedAt = ChinaTime.Now
                        }).ToList();
                        await conn.ExecuteAsync(_sql.Get("Billing.Insert.DebitNoteItem.Default"), itemBatch, tx);
                    }

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
                debitNoteIds.Add((dnId, contract.CompanyId, pYear, pMonth));
                Interlocked.Increment(ref success);
            }
            catch (Exception ex)
            {
                errors.Add((contract.Id, ex.Message));
                Interlocked.Increment(ref fail);
                failedContracts.Add(new BillJobFailedContract(
                    taskLogId, contract.Id, contract.ContractNo ?? "",
                    currentStep, ex.Message));
            }
        });

        // Step06: batch generate PDFs organized by company/year-month
        var pdfSuccess = 0; var pdfFail = 0;
        if (!debitNoteIds.IsEmpty)
        {
            var step06 = await _stepLogger.StartStepAsync(taskLogId, "BillStep06",
                "BatchGeneratePDF", null, null, ct);
            var rootDir = Path.Combine(Path.GetTempPath(), "billpdf");

            // group by company then year-month
            var groups = debitNoteIds
                .GroupBy(x => new { x.CompanyId, x.Year, x.Month })
                .ToList();

            foreach (var g in groups)
            {
                var period = $"{g.Key.Year}-{g.Key.Month:D2}";
                var dir = Path.Combine(rootDir, g.Key.CompanyId.ToString("N"), period);
                Directory.CreateDirectory(dir);
                var batch = g.ToList();

                // process in sub-batches of 1000 for step logging
                int subBatchSize = 1000;
                for (int i = 0; i < batch.Count; i += subBatchSize)
                {
                    var subBatch = batch.Skip(i).Take(subBatchSize).ToList();
                    var batchStep = await _stepLogger.StartStepAsync(taskLogId, "BillStep06_Batch",
                        $"PDF {period} batch {i / subBatchSize + 1}/{batch.Count}",
                        step06, null, ct);

                    await Parallel.ForEachAsync(subBatch,
                        new ParallelOptions { MaxDegreeOfParallelism = _pdfParallelism, CancellationToken = ct },
                        async (item, token) =>
                    {
                        try
                        {
                            var pdfBytes = await _debitNoteService.ExportPdfAsync(item.DnId, token);
                            var filePath = Path.Combine(dir, $"{item.DnId:N}.pdf");
                            await File.WriteAllBytesAsync(filePath, pdfBytes, token);
                            Interlocked.Increment(ref pdfSuccess);
                        }
                        catch
                        {
                            Interlocked.Increment(ref pdfFail);
                        }
                    });

                    await _stepLogger.CompleteStepAsync(batchStep, subBatch.Count, null, ct);
                }
            }

            await _stepLogger.CompleteStepAsync(step06, debitNoteIds.Count, null, ct);
        }

        // persist failed contracts
        if (!failedContracts.IsEmpty)
        {
            try { await _failedContractRepo.CreateBatchAsync(failedContracts, ct); }
            catch { }
        }

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

    private async Task PersistDebitNoteAsync(IDbConnection conn, IDbTransaction tx, DebitNoteRequest req, CancellationToken ct)
    {
        var periodParts = req.Period.Split('-');
        var periodYear = int.Parse(periodParts[0]);
        var periodMonth = int.Parse(periodParts[1]);

        var tenantName = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.Tenant.PrimaryNameByContract"), new { Id = req.ContractId }, tx);
        var buildingAddress = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.HousingUnit.BuildingAddressByContract"), new { Id = req.ContractId }, tx);
        var companyRow = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Organization.Select.Company.ById"), new { Id = req.CompanyId }, tx);
        var companyName = companyRow?.Name as string ?? "";
        var previousBalance = await conn.QuerySingleOrDefaultAsync<decimal>(
            _sql.Get("Billing.Select.Journal.PreviousBalance"),
            new { ContractId = req.ContractId, Year = periodYear, Month = periodMonth }, tx);

        var noteNo = await DebitNoteService.GenerateBillNoAsync(conn, tx);
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.DebitNote.FromBillJob"),
            new
            {
                Id = req.DnId, NoteNo = noteNo,
                CId = req.ContractId, CoId = req.CompanyId, Amt = req.TotalAmount,
                IsHist = req.IsHistorical, Due = req.DueDate,
                PeriodYear = periodYear, PeriodMonth = periodMonth, CBy = Guid.Empty,
                ContractNo = req.ContractNo,
                TenantName = tenantName ?? "",
                BuildingAddress = buildingAddress ?? "",
                CompanyName = companyName ?? "",
                PreviousBalance = previousBalance
            }, tx);
    }

    /// <summary>
    /// DebitNote 创建参数 — 替代 12 个独立参数，减少方法签名复杂度
    /// </summary>
    /// <param name="DnId">DebitNote 主键</param>
    /// <param name="ContractId">合同 ID</param>
    /// <param name="ContractNo">合同编号</param>
    /// <param name="CompanyId">公司 ID</param>
    /// <param name="Period">账期 yyyy-MM</param>
    /// <param name="TotalAmount">账单总金额</param>
    /// <param name="TaskLogId">任务日志 ID</param>
    /// <param name="IsHistorical">是否历史账单</param>
    /// <param name="DueDate">到期日</param>
    private record DebitNoteRequest(
        Guid DnId, Guid ContractId, string ContractNo, Guid CompanyId,
        string Period, decimal TotalAmount, Guid TaskLogId,
        bool IsHistorical = false, DateOnly? DueDate = null);

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
