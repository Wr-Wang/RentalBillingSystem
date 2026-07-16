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
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

/// <summary>
/// 合同控制器 — 合同 CRUD、创建审批、费用调价等
/// DDD 模式：仅依赖 Application Service，不直接操作基础设施
/// </summary>
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
    private readonly ICurrentUserService _currentUser;
    private readonly IReceivableGenerationService _receivableGen;
    private readonly IServiceProvider _serviceProvider;

    public ContractsController(IContractService contractService, IRenewalService renewalService,
        IContractDomainService contractDomainService, IApprovalService approvalService,
        IContractTimelineService timelineService, IUnitOfWork uow,
        ICurrentUserService currentUser,
        IReceivableGenerationService receivableGen,
        IServiceProvider serviceProvider)
    {
        _contractService = contractService;
        _renewalService = renewalService;
        _contractDomainService = contractDomainService;
        _approvalService = approvalService;
        _timelineService = timelineService;
        _uow = uow;
        _currentUser = currentUser;
        _receivableGen = receivableGen;
        _serviceProvider = serviceProvider;
    }

    // =====================================================================
    // GET /api/contracts — 合同列表（分页+筛选）
    // 支持按租客/公司/房屋/关键词/状态筛选
    // =====================================================================
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] Guid? roomId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null, [FromQuery] string? status = null)
    {
        // 按租客查询（快捷查询，不走分页）
        if (tenantId != null)
        {
            var result = await _contractService.GetByTenantIdAsync(tenantId.Value, ct);
            return Ok(new { items = result, total = result.Count });
        }

        if (companyId == null) return Ok(new { items = new List<object>(), total = 0, page = 1, pageSize = 10, totalPages = 0 });
        var pagedResult = await _contractService.GetPagedListAsync(companyId.Value, page, pageSize, keyword, status, roomId, ct);
        return Ok(pagedResult);
    }

    // =====================================================================
    // GET /api/contracts/{id} — 合同详情
    // =====================================================================
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var dto = await _contractService.GetByIdAsync(id, ct);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    // =====================================================================
    // POST /api/contracts — 创建合同
    // 创建成功后写入变更历史（审计追踪）
    // =====================================================================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Application.DTOs.Contract.CreateContractRequest request, CancellationToken ct)
    {
        var dto = await _contractService.CreateAsync(request, ct);
        try
        {
            await _timelineService.InsertChangeHistoryAsync(dto.Id, "CONTRACT_CREATE",
                "合同签订", "新建合同，起租 " + request.StartDate,
                null, null, request.StartDate.ToString("yyyy-MM-dd"), null, ct: ct);
        }
        catch { /* 变更历史写入失败不影响合同创建主流程 */ }
        return Ok(dto);
    }

    // =====================================================================
    // PUT /api/contracts/{id} — 更新合同（起止日期/支付周期/自动续签）
    // 调用 Contract 领域实体的业务方法修改状态，通过 UoW 持久化
    // =====================================================================
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

    /// <summary>合同更新请求体</summary>
    public class ContractUpdateRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? PaymentCycle { get; set; }
        public bool? AutoRenew { get; set; }
    }

    // ===================================================================
    // 合同创建审批（审批驱动模式）
    // ===================================================================

    /// <summary>合同创建审批请求体</summary>
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

    // =====================================================================
    // POST /api/contracts/createrequest — 提交合同创建审批
    // 流程：校验 → 创建暂存请求 → 判断是否需要审批
    //   有审批配置 → 提审批（ContractActivation 类型）
    //   无审批配置 → 直接激活合同（ExecuteContractCreationAsync）
    // =====================================================================
    [HttpPost("createrequest")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateContractRequestDto request, CancellationToken ct)
    {
        // ----- 参数校验 -----
        if (request.RoomId == Guid.Empty) return BadRequest(new { message = "房源不能为空" });
        if (request.TenantIds.Count == 0) return BadRequest(new { message = "必须至少有一个租客" });
        if (request.Fees.Count == 0) return BadRequest(new { message = "必须至少有一个费用配置" });
        if (request.EndDate.HasValue && request.StartDate >= request.EndDate.Value)
            return BadRequest(new { message = "结束日期必须大于开始日期" });

        // ----- 房源防重 -----
        var hasActive = await _uow.Contracts.HasActiveForHousingUnitAsync(request.RoomId, ct);
        if (hasActive) return Conflict(new { message = "该房源已有生效合同" });

        // ----- 创建暂存请求 -----
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

        // ----- 判断审批流程 -----
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_CREATE", ct);
        if (approvalType == null)
        {
            // 无审批配置 → 直接激活
            var contractId = await _contractService.ExecuteContractCreationAsync(createReq.Id, userId, ct);
            return Ok(new { status = "Active", contractId, message = "合同已直接激活" });
        }

        // 有审批配置 → 提交审批（ContractActivation 类型）
        await _contractService.SubmitContractCreateRequestStatusAsync(createReq.Id, ct);
        var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
        {
            ApprovalTypeId = approvalType.Id, Title = $"[合同新建] {contractNo}",
            Description = $"起租 {request.StartDate}" + (request.EndDate.HasValue ? $"，到期 {request.EndDate}" : ""),
            TargetEntityId = createReq.Id, TargetEntityType = "ContractActivation"
        }, ct);
        await _contractService.SetApprovalRequestContractIdAsync(approvalResult.Id, createReq.Id, ct);
        return Ok(new { status = "PendingApproval", requestId = createReq.Id, approvalRequestId = approvalResult.Id });
    }

    // =====================================================================
    // POST /api/contracts/{id}/feeadjust — 费用调价
    // 走审批流或直接执行，由 Application Service 统一编排
    // =====================================================================
    [HttpPost("{id}/feeadjust")]
    public async Task<IActionResult> FeeAdjust(Guid id, [FromBody] FeeAdjustRequest request, CancellationToken ct)
    {
        try
        {
            // 并发守卫：防止同一合同多个调价同时审批
            await _contractService.EnsureNoPendingForContractAsync(id, ct);

            var userId = GetCurrentUserId();
            var result = await _contractService.FeeAdjustAsync(id, request, userId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>从 JWT Claims 获取当前用户 ID</summary>
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }

    // =====================================================================
    // GET /api/contracts/{id}/changes — 获取合同变更历史
    // 用于前端合同详情页的时间线展示
    // =====================================================================
    [HttpGet("{id}/changes")]
    public async Task<IActionResult> GetChanges(Guid id, CancellationToken ct)
    {
        var items = await _timelineService.GetChangesAsync(id, ct);
        return Ok(items);
    }
}
