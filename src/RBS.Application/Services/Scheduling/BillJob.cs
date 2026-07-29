using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.Services.Billing;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Billing;
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
            const int maxRetries = 3;
            for (int retry = 0; retry <= maxRetries; retry++)
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
                    var dueDate = new DateTime(pYear, pMonth, dueDay);

                    var feeConfigs = (await conn.QueryAsync<(Guid Id, Guid FeeCodeId, decimal Amount, string? EffDate, string? ExpDate, string FeeName, string? ChargeType)>(
                        _sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                        new { ContractId = contract.Id, PeriodStart = $"{targetMonth}-01", PeriodEnd = $"{targetMonth}-{lastDay}" }, tx)).ToList();

                    // 拆分当前月开方式费用配置（ExpiryDate=NULL）：关旧开新，确保按月生成
                    var nextMonth = new DateTime(pYear, pMonth, 1).AddMonths(1);
                    var periodEnd = new DateTime(pYear, pMonth, lastDay);
                    var openConfigs = (await conn.QueryAsync<dynamic>(
                        _sql.Get("Scheduling.Select.FeeConfig.OpenActive"),
                        new { Cid = contract.Id }, tx)).ToList();
                    foreach (var oc in openConfigs)
                    {
                        var effDate = (DateTime)oc.EffectiveDate;
                        if (effDate > periodEnd) continue;
                        var coveredDays = (periodEnd - effDate).Days;
                        var totalDays = lastDay;
                        var proratedAmount = Math.Round((decimal)oc.Amount / totalDays * coveredDays, 2);
                        // 关闭当月配置（金额改为已分摊值）
                        await conn.ExecuteAsync(
                            _sql.Get("Scheduling.Update.FeeConfig.CloseByPeriod"),
                            new { End = periodEnd, Amt = proratedAmount, Id = (Guid)oc.Id }, tx);
                        // 新建下月配置（全额）
                        await conn.ExecuteAsync(
                            _sql.Get("Scheduling.Insert.FeeConfig.NextMonth"),
                            new { Cid = contract.Id, FId = (Guid)oc.FeeCodeId, Amt = (decimal)oc.Amount,
                                Mode = (string)oc.BillingMode, Eff = nextMonth, CoId = (Guid)oc.CompanyId,
                                User = Guid.Empty, Now = ChinaTime.Now }, tx);
                    }
                    // 拆分后重新加载费用配置（确保 Step03 使用更新后的起止日期）
                    feeConfigs = (await conn.QueryAsync<(Guid Id, Guid FeeCodeId, decimal Amount, string? EffDate, string? ExpDate, string FeeName, string? ChargeType)>(
                        _sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                        new { ContractId = contract.Id, PeriodStart = $"{targetMonth}-01", PeriodEnd = $"{targetMonth}-{lastDay}" }, tx)).ToList();

                    // 补录已关闭但无 Journal 的周期费用（如 3~7 月手动关闭后 Journal 被清理）
                    var missingRecurring = (await conn.QueryAsync<(Guid Id, Guid FeeCodeId, decimal Amount, string? EffDate, string? ExpDate, string FeeName, string? ChargeType)>(
                        _sql.Get("Scheduling.Select.FeeConfig.RecurringWithoutJournal"),
                        new { Cid = contract.Id, End = periodEnd }, tx)).ToList();
                    if (missingRecurring.Count > 0)
                        feeConfigs.AddRange(missingRecurring);

                    // 补录无 Journal 的一次性费用（如押金）
                    var missingOneTime = (await conn.QueryAsync<(Guid Id, Guid FeeCodeId, decimal Amount, string? EffDate, string? ExpDate, string FeeName, string? ChargeType)>(
                        _sql.Get("Scheduling.Select.FeeConfig.OneTimeWithoutJournal"),
                        new { Cid = contract.Id }, tx)).ToList();
                    if (missingOneTime.Count > 0)
                        feeConfigs.AddRange(missingOneTime);

                    await _stepLogger.CompleteStepAsync(step02, feeConfigs.Count, null, token);

                    // ===== Step03: 生成 Journals + GL + DebitNoteItems（批量收集→批量写入） =====
                currentStep = "BillStep03";
                var step03 = await _stepLogger.StartStepAsync(taskLogId, "BillStep03",
                    $"生成应收-{contractNo}", null, null, token);

                // 生成 journals：拆分时已分摊金额，直接使用每条配置的全额
                var plans = feeConfigs.Select(f => new Journal(
                    companyId: contract.CompanyId,
                    contractId: contract.Id,
                    feeCodeId: f.FeeCodeId,
                    feeConfigId: f.Id,
                    accountingSubjectId: subjects.GetValueOrDefault("1122", Guid.Empty),
                    period: targetMonth,
                    amount: f.Amount,
                    dueDate: dueDate,
                    entryType: f.ChargeType == "OneTime" ? "Deposit" : "Normal",
                    billedAt: ChinaTime.Now,
                    debitNoteId: null,
                    parentJournalId: null,
                    summary: null
                )).ToList();

                // 收集待写入数据（跳过已存在的费用项目）
                var journalBatch = new List<object>(plans.Count);
                var glBatch = new List<object>(plans.Count * 2);
                var itemTuples = new List<(Guid FeeCodeId, decimal Amount, string FeeName, Guid JournalId)>(plans.Count);
                decimal totalAmount = 0;
                int created = 0;

                foreach (var journal in plans)
                {
                    if (journal.Amount <= 0) continue;
                    // 直接查数据库判断是否已存在（FeeConfigId + ContractId + Period 且有有效账单）
                    if (journal.FeeConfigId.HasValue)
                    {
                        var exists = await conn.QuerySingleAsync<int>(
                            _sql.Get("Scheduling.Select.Journal.CheckExistsByFeeConfig"),
                            new { Fid = journal.FeeConfigId.Value, Cid = contract.Id, P = targetMonth }, tx);
                        if (exists > 0)
                        {
                            System.Console.WriteLine($"[DEDUP] 跳过 {contractNo}/{targetMonth}/FeeConfigId={journal.FeeConfigId.Value} - 已有 {exists} 条");
                            continue;
                        }
                        else
                        {
                            System.Console.WriteLine($"[DEDUP] 生成 {contractNo}/{targetMonth}/FeeConfigId={journal.FeeConfigId.Value} - 未找到已存在的记录");
                        }
                    }

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
                    itemTuples.Add((journal.FeeCodeId, journal.Amount, feeName, journal.Id));

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
                    new { CId = contract.Id, P = targetMonth }, tx)).ToList();

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

                    // 检查是否已存在该合同+账期的账单（重跑时跳过）
                    var existingDn = await conn.QuerySingleOrDefaultAsync<Guid?>(
                        _sql.Get("Scheduling.Select.DebitNote.ByContractPeriod"),
                        new { Cid = contract.Id, Y = pYear, M = pMonth }, tx);
                    if (existingDn.HasValue)
                    {
                        dnId = existingDn.Value;
                    }
                    else
                    {
                        var prepaid = contract.PrepaidBalance;
                        dnId = Guid.NewGuid();
                        await PersistDebitNoteAsync(conn, tx, new DebitNoteRequest(
                            dnId, contract.Id, contractNo ?? "", companyId,
                            targetMonth, totalAmount, taskLogId), token);
                    }

                    // 补充该合同+该账期的全部 Journals（不含已在明细中或已在 itemTuples 中的）
                    var existingJournalIds = (await conn.QueryAsync<Guid>(
                        _sql.Get("Scheduling.Select.DebitNoteItem.ExistingJournalIds"),
                        new { DnId = dnId }, tx)).ToHashSet();
                    var step3Ids = itemTuples.Select(t => t.JournalId).ToHashSet();
                    var periodItems = (await conn.QueryAsync<(Guid FeeCodeId, decimal Amount, string FeeName, Guid JournalId)>(
                        _sql.Get("Scheduling.Select.Journal.UnbilledByContractPeriod"),
                        new { Cid = contract.Id, P = targetMonth }, tx))
                        .Where(t => !existingJournalIds.Contains(t.JournalId) && !step3Ids.Contains(t.JournalId))
                        .ToList();
                    if (periodItems.Count > 0)
                        itemTuples = itemTuples.Concat(periodItems).ToList();

                    // 批量写入 DebitNoteItems
                    if (itemTuples.Count > 0)
                    {
                        var itemBatch = itemTuples.Select(t => new
                        {
                            Id = Guid.NewGuid(), DebitNoteId = dnId,
                            FeeCodeId = t.FeeCodeId, FeeName = t.FeeName,
                            Amount = t.Amount, JournalId = t.JournalId,
                            CreatedBy = Guid.Empty, CreatedAt = ChinaTime.Now
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
                break; // 成功后退出重试循环
                }
                catch (Exception ex) when (retry < maxRetries && IsDeadlock(ex))
                {
                    await Task.Delay(1000 * (retry + 1), token);
                    // retry
                }
                catch (Exception ex)
                {
                    errors.Add((contract.Id, ex.Message));
                    Interlocked.Increment(ref fail);
                    failedContracts.Add(new BillJobFailedContract(
                        taskLogId, contract.Id, contract.ContractNo ?? "",
                        currentStep, ex.Message));
                    break; // exit retry loop on non-deadlock or max retries
                }
            } // end retry for
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
        bool IsHistorical = false, DateTime? DueDate = null);

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

    private static bool IsDeadlock(Exception ex)
    {
        var msg = ex.Message;
        if (msg.Contains("死锁") || msg.Contains("deadlock") || msg.Contains("1205"))
            return true;
        return ex.InnerException != null && IsDeadlock(ex.InnerException);
    }
}
