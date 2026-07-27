using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 日记账应用服务实现 — 封装日记账查询、预览、生成的编排逻辑
/// </summary>
public class JournalAppService : IJournalAppService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    private readonly ICurrentUserService _currentUser;
    private readonly IApprovalService _approvalService;
    private readonly IReceivableGenerationService _receivableGen;

    public JournalAppService(
        IDbConnectionFactory db,
        ISqlLoader sql,
        IUnitOfWork uow,
        ITenantService tenant,
        ICurrentUserService currentUser,
        IApprovalService approvalService,
        IReceivableGenerationService receivableGen)
    {
        _db = db;
        _sql = sql;
        _uow = uow;
        _tenant = tenant;
        _currentUser = currentUser;
        _approvalService = approvalService;
        _receivableGen = receivableGen;
    }

    public async Task<object> GetPagedAsync(Guid? companyId, string? period, string? contractNo, Guid? feeCodeId, bool? glPosted, Guid? contractId, int page, int pageSize)
    {
        var effectiveCompanyId = companyId ?? _tenant.EffectiveCompanyId;
        if (effectiveCompanyId == null) return new { items = new List<object>(), total = 0 };

        using var conn = _db.CreateConnection();
        conn.Open();
        var items = await conn.QueryAsync(_sql.Get("Billing.Select.Journal.Paged"),
            new { CoId = effectiveCompanyId, Period = period, CNo = $"%{contractNo}%", FId = feeCodeId, GLP = glPosted, CId = contractId, Offset = (page - 1) * pageSize, PageSize = pageSize });
        var total = await conn.QuerySingleAsync<int>(_sql.Get("Billing.Select.Journal.PagedCount"),
            new { CoId = effectiveCompanyId, Period = period, CNo = $"%{contractNo}%", FId = feeCodeId, GLP = glPosted, CId = contractId });
        return new { items, total, page, pageSize };
    }

    public async Task<object?> GetByIdAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var item = await conn.QuerySingleOrDefaultAsync(
            _sql.Get("Billing.Select.Journal.ById"), new { Id = id });
        return item;
    }

    public async Task<List<object>> GetByContractAsync(Guid contractId)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var items = await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.ByContractWithPayment"),
            new { CId = contractId });
        return items.Cast<object>().ToList();
    }

    /// <summary>
    /// 应收预览明细 DTO — 共享于 PreviewAsync 和 GenerateRequestAsync 之间
    /// </summary>
    private record PreviewItem(
        Guid FeeCodeId,
        string FeeName,
        string Period,
        decimal Amount,
        DateTime DueDate,
        string ChargeType,
        string? EffStart = null,
        string? EffEnd = null
    );

    /// <summary>
    /// 计算待生成的应收明细（共享方法，供 PreviewAsync 和 GenerateRequestAsync 复用）
    /// </summary>
    private async Task<List<PreviewItem>> ComputePreviewItemsAsync(Guid contractId)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId);
        if (contract == null || contract.Status != "Active") return new List<PreviewItem>();

        var allPeriods = ReceivableGenerationService.SplitPeriodsStatic(contract);

        using var conn = _db.CreateConnection();
        conn.Open();

        // 已存在 Journal 的 (Period, FeeCodeId) 组合，用于细粒度去重
        var existingRows = (await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.PeriodFeePairsByContract"),
            new { CId = contractId })).ToList();
        var existingPairs = new HashSet<(string Period, Guid FeeCodeId)>();
        foreach (dynamic row in existingRows)
        {
            existingPairs.Add(((string)row.Period, (Guid)row.FeeCodeId));
        }
        var existingFeeCodeIds = existingPairs.Select(p => p.FeeCodeId).ToHashSet();

        // 加载所有费用配置（含 ChargeType）
        var allFeesDynamic = (await conn.QueryAsync(
            _sql.Get("Lease.Select.ContractFeeConfig.WithFeeCodeByContract"),
            new { ContractId = contractId })).ToList();
        var allFees = allFeesDynamic.Select(x => (
            FeeCodeId: (Guid)x.FeeCodeId,
            Amount: (decimal)x.Amount,
            EffectiveDate: x.EffectiveDate is DateTime ed ? ed.ToString("yyyy-MM-dd") : null,
            ExpiryDate: x.ExpiryDate is DateTime xd ? xd.ToString("yyyy-MM-dd") : null,
            Name: (string)x.Name,
            ChargeType: (string)x.ChargeType,
            IsActive: (bool)x.IsActive
        )).ToList();

        var activeFees = allFees.Where(f => f.IsActive || (!f.IsActive && f.ExpiryDate != null)).ToList();

        // 获取 BillJob 已执行到的最后月份（周期收费只生成到该月为止）
        var maxBilledMonth = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Scheduling.Select.Execution.MaxBilledMonthByCompany"),
            new { CompanyId = contract.CompanyId, CurrentMonth = ChinaTime.Now.ToString("yyyy-MM") });

        var items = new List<PreviewItem>();
        var oneTimeShown = new HashSet<Guid>();

        foreach (var period in allPeriods)
        {
            var year = int.Parse(period[..4]);
            var month = int.Parse(period[5..7]);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var periodStart = new DateTime(year, month, 1);
            var periodEnd = new DateTime(year, month, daysInMonth);
            var dueDay = contract.EndDate.HasValue ? Math.Min(contract.EndDate.Value.Day, daysInMonth) : daysInMonth;
            var dueDate = new DateTime(year, month, dueDay);

            // 1) Recurring 费用 — 按天分摊（仅限 BillJob 已执行到的月份为止）
            if (string.Compare(period, maxBilledMonth, StringComparison.Ordinal) > 0)
                break;

            var recurring = activeFees.Where(f => f.ChargeType == "Recurring").ToList();
            foreach (var group in recurring.GroupBy(f => f.FeeCodeId))
            {
                if (existingPairs.Contains((period, group.Key)))
                    continue;

                decimal prorated = 0;
                DateTime? groupEffStart = null, groupEffEnd = null;

                foreach (var fc in group)
                {
                    var effStart = fc.EffectiveDate != null
                        ? DateTime.Parse(fc.EffectiveDate) : periodStart;
                    var effEnd = fc.ExpiryDate != null
                        ? DateTime.Parse(fc.ExpiryDate) : periodEnd;
                    var overlapStart = effStart > periodStart ? effStart : periodStart;
                    var overlapEnd = effEnd < periodEnd ? effEnd : periodEnd;
                    var coveredDays = overlapStart <= overlapEnd
                        ? (overlapEnd - overlapStart).Days + 1 : 0;
                    if (coveredDays > 0)
                    {
                        prorated += Math.Round(fc.Amount / daysInMonth * coveredDays, 2);
                        if (groupEffStart == null || overlapStart < groupEffStart) groupEffStart = overlapStart;
                        if (groupEffEnd == null || overlapEnd > groupEffEnd) groupEffEnd = overlapEnd;
                    }
                }
                prorated = Math.Round(prorated, 2);
                if (prorated > 0)
                {
                    items.Add(new PreviewItem(
                        group.Key, group.First().Name, period, prorated, dueDate, "Recurring",
                        groupEffStart?.ToString("yyyy-MM-dd"), groupEffEnd?.ToString("yyyy-MM-dd")
                    ));
                }
            }

            // 2) OneTime 费用
            var oneTime = activeFees
                .Where(f => f.ChargeType == "OneTime" && !existingFeeCodeIds.Contains(f.FeeCodeId))
                .Where(f => oneTimeShown.Add(f.FeeCodeId))
                .ToList();
            foreach (var fc in oneTime)
            {
                items.Add(new PreviewItem(
                    fc.FeeCodeId, fc.Name, period, fc.Amount,
                    ChinaTime.Now.AddDays(30), "OneTime"
                ));
            }
        }

        return items;
    }

    /// <summary>预览生成应收 — 计算哪些账期缺少 Journal（委托给 ComputePreviewItemsAsync）</summary>
    public async Task<object> PreviewAsync(Guid contractId)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId);
        var allPeriods = contract != null
            ? ReceivableGenerationService.SplitPeriodsStatic(contract)
            : new List<string>();

        var items = await ComputePreviewItemsAsync(contractId);
        var totalAmount = items.Sum(i => i.Amount);

        return new
        {
            items = items.Select(i => new
            {
                period = i.Period,
                feeName = i.FeeName,
                fullAmount = i.Amount,
                amount = i.Amount,
                dueDate = i.DueDate.ToString("yyyy-MM-dd"),
                chargeType = i.ChargeType,
                effStart = i.EffStart,
                effEnd = i.EffEnd
            }),
            totalAmount,
            missingCount = allPeriods.Count
        };
    }

    public async Task<object> GenerateRequestAsync(Guid contractId, Guid userId)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId);
        if (contract == null) return new { message = "合同不存在" };

        // 检查是否有应收生成审批类型
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("RECEIVABLE_GENERATE");
        if (approvalType != null && contract.Status == "Active")
        {
            // 并发守卫：同一合同不能有多个待审批的应收生成请求
            using (var guardConn = _db.CreateConnection())
            {
                guardConn.Open();
                var pending = await guardConn.QuerySingleAsync<int>(
                    _sql.Get("ReceivableGenerate.Select.Request.PendingCountByContract"),
                    new { CId = contractId });
                if (pending > 0)
                    throw new InvalidOperationException("该合同已有待审批的应收生成请求，请处理完成后再提交");
            }

            // 1. 计算待生成的应收明细（复用预览逻辑）
            var previewItems = await ComputePreviewItemsAsync(contractId);
            if (previewItems.Count == 0)
                return new { message = "无可生成的应收项目，所有账期已生成", count = 0 };

            var allPeriods = ReceivableGenerationService.SplitPeriodsStatic(contract);
            var totalAmount = previewItems.Sum(i => i.Amount);

            // 2. 创建 ReceivableGenerateRequest 聚合（初始状态 Draft，防止审批失败导致假锁）
            var request = new ReceivableGenerateRequest(
                contractId, contract.CompanyId, allPeriods.First(), allPeriods.Last());
            request.SetCreated(userId, ChinaTime.Now, null, null);
            await _uow.ReceivableGenerateRequests.AddAsync(request);

            // 3. 创建 ReceivableGenerateRequestItem 子实体（使用 SqlMap 写入）
            foreach (var item in previewItems)
            {
                var requestItemId = Guid.NewGuid();
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("ReceivableGenerate.Insert.Item.Default"),
                    new
                    {
                        Id = requestItemId,
                        RequestId = request.Id,
                        FeeCodeId = item.FeeCodeId,
                        FeeName = item.FeeName,
                        Period = item.Period,
                        Amount = item.Amount,
                        DueDate = item.DueDate,
                        EntryType = "Normal"
                    });
            }

            // 4. 提交审批（TargetEntityId = 聚合ID）
            var approvalResult = await _approvalService.SubmitAsync(new DTOs.Approval.SubmitApprovalRequest
            {
                ApprovalTypeId = approvalType.Id,
                Title = $"[应收生成] {contract.ContractNo}",
                Description = $"手动触发生成应收，合计 ¥{totalAmount:N2}",
                TargetEntityId = request.Id,
                TargetEntityType = "ReceivableGeneration"
            });

            // 5. 审批提交成功后，将聚合状态从 Draft 提升为 PendingApproval 并关联审批ID
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("ReceivableGenerate.Update.Request.Submit"),
                new { Id = request.Id, ApprovalRequestId = approvalResult.Id });

            // 6. 写入审批业务数据（供审批详情页展示）
            var bizDataId = Guid.NewGuid();
            var lastPeriod = previewItems.Last().Period;
            var periodEndDate = $"{lastPeriod}-{DateTime.DaysInMonth(int.Parse(lastPeriod[..4]), int.Parse(lastPeriod[5..7])):D2}";
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("ReceivableGenerate.Insert.ApprovalBizData.Default"),
                new
                {
                    Id = bizDataId,
                    ApprovalRequestId = approvalResult.Id,
                    ContractId = contractId,
                    ContractNo = contract.ContractNo,
                    CompanyId = contract.CompanyId,
                    NewAmount = totalAmount,
                    Reason = $"{contract.StartDate:yyyy-MM-dd} ~ {periodEndDate}",
                    CreatedBy = userId,
                    CreatedAt = ChinaTime.Now
                });

            await _uow.CommitAsync();
            return new { status = "PendingApproval", id = approvalResult.Id, message = "应收生成请求已提交审批" };
        }

        // 无审批配置或 Draft 合同 → 直接执行
        var created = await _receivableGen.GenerateAsync(contractId, null, null);
        if (created > 0)
        {
            var now = ChinaTime.Now;
            var period = $"{now.Year}-{now.Month:D2}";
            using var conn = _db.CreateConnection();
            conn.Open();

            // 1. 更新合同欠款余额
            var totalBilled = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Billing.Select.Journal.SumAmountByContractPeriod"),
                new { CId = contractId, P = period });
            await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                new { Id = contractId, Amt = totalBilled });
        }
        return new { message = $"已生成 {created} 条应收记录", count = created };
    }

    public async Task<object> PostAsync(List<Guid> ids)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        int count = 0;
        var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes"), null, tx)).ToDictionary(r => r.Code, r => r.Id);

        foreach (var id in ids)
        {
            var updated = await conn.ExecuteAsync(
                _sql.Get("Billing.Update.Journal.Post"), new { Id = id }, tx);
            if (updated == 0) continue;

            var j = await conn.QuerySingleOrDefaultAsync<dynamic>(
                _sql.Get("Billing.Select.Journal.WithContractNo"),
                new { Id = id }, tx);
            if (j == null) continue;
            var jAmt = (decimal)(j.Amount ?? 0);
            if (jAmt <= 0) continue;

            // 写 GL 分录
            var coId = (Guid)(j.CompanyId ?? Guid.Empty);
            var cId = (Guid)(j.ContractId ?? Guid.Empty);
            var period = (string)(j.Period ?? "");
            var cNo = (string)(j.ContractNo ?? "");
            if (coId == Guid.Empty || cId == Guid.Empty) continue;

            if (subjects.ContainsKey("1122"))
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                    new { Id = Guid.NewGuid(), CoId = coId, CId = cId,
                        CNo = cNo, Period = period,
                        SId = subjects["1122"], SCode = "1122", Dir = "Debit",
                        Amt = jAmt, SrcType = "JournalPost", SrcId = id,
                        Desc = "", CBy = Guid.Empty }, tx);
            if (subjects.ContainsKey("6001"))
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                    new { Id = Guid.NewGuid(), CoId = coId, CId = cId,
                        CNo = cNo, Period = period,
                        SId = subjects["6001"], SCode = "6001", Dir = "Credit",
                        Amt = jAmt, SrcType = "JournalPost", SrcId = id,
                        Desc = "", CBy = Guid.Empty }, tx);

            // 过账时同步增加合同欠款余额
            await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                new { Id = cId, Amt = jAmt }, tx);

            // 过账时若有预存余额，自动抵扣（预存抵应收，不碰收入科目）
            var prepaidBalance = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Contract.Select.Contract.PrepaidBalance"),
                new { Id = cId }, tx);
            if (prepaidBalance > 0m)
            {
                var applyAmt = Math.Min(prepaidBalance, jAmt);
                if (applyAmt > 0m && subjects.ContainsKey("2203") && subjects.ContainsKey("1122"))
                {
                    // Dr 2203(预收账款)
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                        new { Id = Guid.NewGuid(), CoId = coId, CId = cId,
                            CNo = cNo, Period = period,
                            SId = subjects["2203"], SCode = "2203", Dir = "Debit",
                            Amt = applyAmt, SrcType = "JournalPost", SrcId = id,
                            Desc = "", CBy = Guid.Empty }, tx);
                    // Cr 1122(应收账款)
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                        new { Id = Guid.NewGuid(), CoId = coId, CId = cId,
                            CNo = cNo, Period = period,
                            SId = subjects["1122"], SCode = "1122", Dir = "Credit",
                            Amt = applyAmt, SrcType = "JournalPost", SrcId = id,
                            Desc = "", CBy = Guid.Empty }, tx);
                    // 调整合同余额：欠款减少、预存减少
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                        new { Id = cId, Amt = -applyAmt }, tx);
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.PrepaidBalanceDecrement"),
                        new { Id = cId, Amt = applyAmt }, tx);
                }
            }

            count++;
        }

        tx.Commit();
        return new { posted = count };
    }
}
