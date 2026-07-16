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
        DateOnly DueDate,
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

        var items = new List<PreviewItem>();
        var oneTimeShown = new HashSet<Guid>();

        foreach (var period in allPeriods)
        {
            var year = int.Parse(period[..4]);
            var month = int.Parse(period[5..7]);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var periodStart = new DateOnly(year, month, 1);
            var periodEnd = new DateOnly(year, month, daysInMonth);
            var dueDay = contract.EndDate.HasValue ? Math.Min(contract.EndDate.Value.Day, daysInMonth) : daysInMonth;
            var dueDate = new DateOnly(year, month, dueDay);

            // 1) Recurring 费用 — 按天分摊
            var recurring = activeFees.Where(f => f.ChargeType == "Recurring").ToList();
            foreach (var group in recurring.GroupBy(f => f.FeeCodeId))
            {
                if (existingPairs.Contains((period, group.Key)))
                    continue;

                decimal prorated = 0;
                DateOnly? groupEffStart = null, groupEffEnd = null;

                foreach (var fc in group)
                {
                    var effStart = fc.EffectiveDate != null
                        ? DateOnly.Parse(fc.EffectiveDate) : periodStart;
                    var effEnd = fc.ExpiryDate != null
                        ? DateOnly.Parse(fc.ExpiryDate) : periodEnd;
                    var overlapStart = effStart > periodStart ? effStart : periodStart;
                    var overlapEnd = effEnd < periodEnd ? effEnd : periodEnd;
                    var coveredDays = overlapStart <= overlapEnd
                        ? overlapEnd.DayNumber - overlapStart.DayNumber + 1 : 0;
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
                    DateOnly.FromDateTime(ChinaTime.Now.AddDays(30)), "OneTime"
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
            var periodEndDate = contract.EndDate.HasValue
                ? contract.EndDate.Value.ToString("yyyy-MM-dd")
                : $"{allPeriods.Last()}-{DateTime.DaysInMonth(int.Parse(allPeriods.Last()[..4]), int.Parse(allPeriods.Last()[5..7])):D2}";
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

            // 2. 更新 GL（追加式快照）
            var latest = await conn.QuerySingleOrDefaultAsync(
                _sql.Get("Accounting.Select.GL.LatestByPeriod"),
                new { CoId = contract.CompanyId, Period = period });
            var prevBilled = latest != null ? (decimal)((dynamic)latest).TotalBilled : 0m;
            var prevReceived = latest != null ? (decimal)((dynamic)latest).TotalReceived : 0m;
            var opening = latest != null ? (decimal)((dynamic)latest).OpeningBalance : 0m;
            var newBilled = prevBilled + totalBilled;
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Default"),
                new
                {
                    Id = Guid.NewGuid(), CoId = contract.CompanyId, Period = period,
                    Opening = opening, Billed = newBilled, Received = prevReceived,
                    Closing = opening + newBilled - prevReceived, CBy = Guid.Empty
                });
        }
        return new { message = $"已生成 {created} 条应收记录", count = created };
    }
}
