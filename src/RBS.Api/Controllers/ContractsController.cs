using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Application.DTOs.Contract;
using RBS.Application.Services.Contract;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IRenewalService _renewalService;
    private readonly IContractDomainService _contractDomainService;
    private readonly IApprovalService _approvalService;
    private readonly IContractTimelineService _timelineService;
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ICurrentUserService _currentUser;
    private readonly IReceivableGenerationService _receivableGen;
    private readonly IServiceProvider _serviceProvider;

    public ContractsController(IContractService contractService, IRenewalService renewalService,
        IContractDomainService contractDomainService, IApprovalService approvalService,
        IContractTimelineService timelineService, IUnitOfWork uow,
        IDbConnectionFactory db, ISqlLoader sql, ICurrentUserService currentUser,
        IReceivableGenerationService receivableGen,
        IServiceProvider serviceProvider)
    {
        _contractService = contractService;
        _renewalService = renewalService;
        _contractDomainService = contractDomainService;
        _approvalService = approvalService;
        _timelineService = timelineService;
        _uow = uow;
        _db = db;
        _sql = sql;
        _currentUser = currentUser;
        _receivableGen = receivableGen;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] Guid? roomId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null, [FromQuery] string? status = null)
    {
        if (tenantId != null)
        {
            var result = await _contractService.GetByTenantIdAsync(tenantId.Value, ct);
            return Ok(new { items = result, total = result.Count });
        }

        if (companyId == null) return Ok(new { items = new List<object>(), total = 0, page = 1, pageSize = 10, totalPages = 0 });
        var pagedResult = await _contractService.GetPagedListAsync(companyId.Value, page, pageSize, keyword, status, roomId, ct);
        return Ok(pagedResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var dto = await _contractService.GetByIdAsync(id, ct);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Application.DTOs.Contract.CreateContractRequest request, CancellationToken ct)
    {
        var dto = await _contractService.CreateAsync(request, ct);
        try { using var conn = _db.CreateConnection(); conn.Open();
            await InsertChangeHistoryAsync(conn, null, dto.Id, "CONTRACT_CREATE",
                "合同签订", "新建合同，起租 " + request.StartDate,
                null, null, request.StartDate.ToString("yyyy-MM-dd"), null); } catch { }
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ContractUpdateRequest request, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();

        if (request.StartDate.HasValue && request.EndDate.HasValue)
            entity.SetPeriod(request.StartDate.Value, request.EndDate.Value);
        if (!string.IsNullOrEmpty(request.PaymentCycle)) entity.SetPaymentCycle(request.PaymentCycle);
        if (request.AutoRenew.HasValue) entity.ConfigureAutoRenew(request.AutoRenew.Value);

        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    public class ContractUpdateRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? PaymentCycle { get; set; }
        public bool? AutoRenew { get; set; }
    }

    // ===================================================================
    // 合同创建审批（�芯� 新增—审批驱动）
    // ===================================================================
    public class CreateContractRequestDto
    {
        public Guid RoomId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string PaymentCycle { get; set; } = "Monthly";
        public Guid CompanyId { get; set; }
        public List<Guid> TenantIds { get; set; } = new();
        public List<CreateContractFeeDto> Fees { get; set; } = new();
        public string? Remark { get; set; }
    }

    public class CreateContractFeeDto
    {
        public Guid FeeCodeId { get; set; }
        public decimal Amount { get; set; }
        public string BillingMode { get; set; } = "FixedAmount";
        public string ChargeType { get; set; } = "Recurring";
        public string? EffectiveDate { get; set; }
    }

    [HttpPost("createrequest")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateContractRequestDto request, CancellationToken ct)
    {
        if (request.RoomId == Guid.Empty) return BadRequest(new { message = "房源不能为空" });
        if (request.TenantIds.Count == 0) return BadRequest(new { message = "必须至少有一个租客" });
        if (request.Fees.Count == 0) return BadRequest(new { message = "必须至少有一个费用配置" });
        // EndDate 选填
        if (request.EndDate.HasValue && request.StartDate >= request.EndDate.Value)
            return BadRequest(new { message = "结束日期必须大于开始日期" });

        var hasActive = await _uow.Contracts.HasActiveForHousingUnitAsync(request.RoomId, ct);
        if (hasActive) return Conflict(new { message = "该房源已有生效合同" });

        var userId = GetCurrentUserId();
        var now = ChinaTime.Now;
        var contractNo = $"CT-{now:yyyyMMdd}-{Guid.NewGuid():N}"[..32];

        var createReq = new ContractCreateRequest(contractNo, request.RoomId, request.StartDate, request.EndDate, request.PaymentCycle, request.CompanyId);
        createReq.SetCreated(userId, now, null, null);
        await _uow.ContractCreateRequests.AddAsync(createReq, ct);

        foreach (var tid in request.TenantIds)
        {
            var t = new ContractCreateRequestTenant(createReq.Id, tid, true);
            await _uow.ContractCreateRequestTenants.AddAsync(t, ct);
        }
        foreach (var f in request.Fees)
        {
            var fee = new ContractCreateRequestFee(createReq.Id, f.FeeCodeId, f.Amount, f.BillingMode, f.ChargeType, f.EffectiveDate);
            await _uow.ContractCreateRequestFees.AddAsync(fee, ct);
        }
        await _uow.CommitAsync(ct);

        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_CREATE", ct);
        if (approvalType == null)
        {
            await ExecuteContractCreationAsync(createReq.Id, userId, ct);
            return Ok(new { status = "Active", contractId = createReq.NewContractId, message = "合同已直接激活" });
        }

        // 直接 SQL 更新状态（CommitAsync 已清空追踪缓存，Submit() 的内存变更不会自动持久化）
        await _uow.ExecuteSqlRawAsync(_sql.Get("ContractCreate.Update.Request.Submit"),
            new { Id = createReq.Id }, ct);
        var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
        {
            ApprovalTypeId = approvalType.Id, Title = $"[合同新建] {contractNo}",
            Description = $"起租 {request.StartDate}" + (request.EndDate.HasValue ? $"，到期 {request.EndDate}" : ""),
            TargetEntityId = createReq.Id, TargetEntityType = "ContractActivation"
        }, ct);
        await _uow.ExecuteSqlRawAsync(_sql.Get("Approval.Update.Request.SetContractId"),
            new { Id = approvalResult.Id, ContractId = createReq.Id }, ct);
        return Ok(new { status = "PendingApproval", requestId = createReq.Id, approvalRequestId = approvalResult.Id });
    }

    private async Task<Guid> ExecuteContractCreationAsync(Guid requestId, Guid userId, CancellationToken ct)
    {
        var request = await _uow.ContractCreateRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new InvalidOperationException("请求不存在");

        var allTenants = await _uow.ContractCreateRequestTenants.GetAllAsync(ct);
        var tenants = allTenants.Where(t => t.RequestId == requestId).ToList();
        var allFees = await _uow.ContractCreateRequestFees.GetAllAsync(ct);
        var feeList = allFees.Where(f => f.RequestId == requestId).ToList();
        var now = ChinaTime.Now;
        var contractId = Guid.NewGuid();

        await _uow.ExecuteSqlRawAsync(_sql.Get("Lease.Insert.Contract.Default"),
            new { Id = contractId, ContractNo = request.ContractNo, RoomId = request.RoomId,
                StartDate = request.StartDate, EndDate = request.EndDate,
                PaymentCycle = request.PaymentCycle, Status = "Active", CompanyId = request.CompanyId,
                CreatedBy = userId, CreatedAt = now }, ct);
        foreach (var t in tenants)
            await _uow.ExecuteSqlRawAsync(_sql.Get("Lease.Insert.ContractTenant.Default"),
                new { ContractId = contractId, t.TenantId, t.IsPrimary,
                    CreatedBy = userId, CreatedAt = now }, ct);
        foreach (var f in feeList)
            await _uow.ExecuteSqlRawAsync(_sql.Get("Lease.Insert.ContractFeeConfig.Default"),
                new { Id = Guid.NewGuid(), ContractId = contractId, f.FeeCodeId,
                    BillingMode = f.BillingMode, Amount = f.Amount,
                    EffectiveDate = f.EffectiveDate ?? request.StartDate.ToString("yyyy-MM-dd"),
                    CreatedBy = userId, CreatedAt = now }, ct);

        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract != null) { contract.Activate(); await _uow.CommitAsync(ct); }
        try { await _receivableGen.GenerateForActivationAsync(contractId, ct); } catch { }
        return contractId;
    }

    // ===================================================================
    // 租金调整（★ v3 改造 — 审批驱动）
    // ===================================================================

    // ===================================================================
    // 费用调价（★ v3 改造 — 审批驱动）
    // ===================================================================
    [HttpPost("{id}/feeadjust")]
    public async Task<IActionResult> FeeAdjust(Guid id, [FromBody] FeeAdjustRequest request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();

        var userId = GetCurrentUserId();

        // 并发守卫
        var conflict = await EnsureNoPendingForContractAsync(id, ct);
        if (conflict != null) return conflict;

        // 找审批类型
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_FEE_CHANGE", ct);

        // 无审批配置 → 0 级直接执行
                if (approvalType == null)
        {
            using var innerConn = _db.CreateConnection(); innerConn.Open();
            using var innerTx = innerConn.BeginTransaction();
            try
            {
                foreach (var item in request.Items)
                {
                    var effDate = item.EffectiveDate ?? "";
                    if (string.IsNullOrEmpty(effDate)) continue;

                    var current = await innerConn.QuerySingleOrDefaultAsync(
                        _sql.Get("Lease.Select.ContractFeeConfig.CurrentByContractAndFee"),
                        new { ContractId = id, FeeCodeId = item.FeeCodeId }, innerTx);

                    var overlap = await innerConn.QuerySingleAsync<int>(
                        _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
                        new { ContractId = id, FeeCodeId = item.FeeCodeId,
                            EffectiveDate = effDate, ExpiryDate = (string?)null,
                            ExcludeId = current != null ? (Guid)((dynamic)current).Id : (Guid?)null }, innerTx);
                    if (overlap > 0)
                        throw new InvalidOperationException("费用项 " + item.FeeName + " 的生效日期与已有记录冲突");

                    var expiryDate = DateTime.Parse(effDate).AddDays(-1).ToString("yyyy-MM-dd");
                    if (current != null)
                    {
                        await innerConn.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.ExpiryDate"),
                            new { Id = (Guid)((dynamic)current).Id, ExpiryDate = expiryDate }, innerTx);
                        await innerConn.ExecuteAsync(_sql.Get("Contract.Update.ContractFeeConfig.ExpireByCodeId"),
                            new { ExpiryDate = expiryDate, ContractId = id, FeeCodeId = item.FeeCodeId }, innerTx);
                    }
                    await innerConn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
                        new { Id = Guid.NewGuid(), ContractId = id, FeeCodeId = item.FeeCodeId,
                            BillingMode = item.BillingMode ?? "FixedAmount", Amount = item.NewAmount,
                            EffectiveDate = effDate, CreatedBy = userId, Now = ChinaTime.Now }, innerTx);
                }
                innerTx.Commit();
            }
            catch (Exception ex)
            {
                innerTx.Rollback();
                return BadRequest(new { error = "[Tx] " + ex.Message });
            }

            // 以下为 Commit 后的操作（独立 try，失败不影响调价）
            try
            {
                foreach (var item in request.Items)
                {
                    var effDateStr = item.EffectiveDate ?? "";
                    if (!string.IsNullOrEmpty(effDateStr))
                        await InsertChangeHistoryAsync(_db.CreateConnection(), null, id, "FEE_ADJUST",
                            "费用调价", item.FeeName + ": " + item.OldAmount.ToString("F2") + " -> " + item.NewAmount.ToString("F2") + "，生效 " + effDateStr,
                            item.OldAmount, item.NewAmount, effDateStr, userId);
                }

                var currentMonth = DateOnly.FromDateTime(ChinaTime.Now).ToString("yyyy-MM");
                foreach (var item in request.Items)
                {
                    var effDateStr = item.EffectiveDate ?? "";
                    if (!string.IsNullOrEmpty(effDateStr))
                    {
                        // 调价补差：INSERT Journal + Update GL
                        var diffAmt = Math.Round(item.NewAmount - item.OldAmount, 2);
                        if (diffAmt != 0)
                        {
                            var subj = await innerConn.QuerySingleOrDefaultAsync(
                                _sql.Get("Accounting.Select.Subject.ByCode"), new { Code = "1122" });
                            var sId = subj != null ? (Guid)((dynamic)subj).Id : Guid.Empty;
                            var effM = effDateStr.Substring(0, 7);
                            await innerConn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                                new { Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = id,
                                    FId = item.FeeCodeId, FConfigId = (Guid?)null, SubjId = sId,
                                    Period = effM, Amt = Math.Abs(diffAmt),
                                    Due = DateOnly.FromDateTime(ChinaTime.Now).AddDays(30),
                                    EntryType = "Supplementary", BilledAt = ChinaTime.Now,
                                    DNId = (Guid?)null, ParentId = (Guid?)null,
                                    Summary = $"{item.FeeName}调价补差", CBy = userId }, innerTx);
                            // TODO: GL 更新 — 原 glLatest 逻辑已移除
                        }
                    }
                }
            }
            catch { /* TODO: 补差 JE 失败不阻断调价 */ }
        }

        // TODO: 审批流程（approvalType != null）
        return Ok(new { contractId = id });
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }

    private async Task<IActionResult?> EnsureNoPendingForContractAsync(Guid contractId, CancellationToken ct)
    {
        var pending = await _uow.ExecuteSqlRawAsync(
            _sql.Get("Approval.Select.Request.HasPendingByContract"),
            new { ContractId = contractId, TargetEntityType = "ContractFeeAdjust" }, ct);
        if (pending > 0)
            return Conflict(new { message = "该合同已有待审批的调价申请" });
        return null;
    }

    private async Task InsertChangeHistoryAsync(IDbConnection conn, IDbTransaction? tx,
        Guid contractId, string changeType, string title, string detail,
        decimal? oldValue, decimal? newValue, string? effectiveDate, Guid? operatorId, string? operatorName = null)
    {
        if (string.IsNullOrEmpty(operatorName) && operatorId.HasValue)
            try { operatorName = await conn.QuerySingleOrDefaultAsync<string>(
                _sql.Get("Contract.Select.User.DisplayNameById"), new { Id = operatorId }, tx); } catch { }
        await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
            new { Id = Guid.NewGuid(), ContractId = contractId, ChangeType = changeType, Title = title,
                Detail = detail, OldValue = oldValue, NewValue = newValue, EffectiveDate = effectiveDate,
                OperatorId = operatorId, OperatorName = operatorName ?? "" }, tx);
    }
}
