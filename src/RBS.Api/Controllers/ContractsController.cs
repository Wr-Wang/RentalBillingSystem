using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Application.Services.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IRenewalService _renewalService;
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly IApprovalService _approvalService;
    private readonly IContractTimelineService _timelineService;
    public ContractsController(IContractService contractService, IRenewalService renewalService,
        IUnitOfWork uow, IDbConnectionFactory db, IApprovalService approvalService,
        IContractTimelineService timelineService)
    {
        _contractService = contractService;
        _renewalService = renewalService;
        _uow = uow;
        _db = db;
        _approvalService = approvalService;
        _timelineService = timelineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null, [FromQuery] string? status = null)
    {
        if (tenantId != null)
        {
            var result = await _contractService.GetByTenantIdAsync(tenantId.Value, ct);
            return Ok(new { items = result, total = result.Count });
        }

        if (companyId == null) return Ok(new { items = new List<object>(), total = 0, page = 1, pageSize = 10, totalPages = 0 });
        var result2 = await _contractService.GetPagedListAsync(companyId.Value, page, pageSize, keyword, status, ct);
        return Ok(result2);
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
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ContractUpdateRequest request, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();

        if (request.RentAmount.HasValue) entity.SetRentAmount(request.RentAmount.Value);
        if (request.DepositAmount.HasValue) entity.SetDepositAmount(request.DepositAmount.Value);
        if (request.StartDate.HasValue && request.EndDate.HasValue)
            entity.SetPeriod(request.StartDate.Value, request.EndDate.Value);
        if (!string.IsNullOrEmpty(request.PaymentCycle)) entity.SetPaymentCycle(request.PaymentCycle);
        if (request.AutoRenew.HasValue) entity.SetAutoRenew(request.AutoRenew.Value);

        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    public class ContractUpdateRequest
    {
        public decimal? RentAmount { get; set; }
        public decimal? DepositAmount { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? PaymentCycle { get; set; }
        public bool? AutoRenew { get; set; }
    }

    [HttpPost("{id}/terminate")]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] Dictionary<string, string> body, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();

        body.TryGetValue("reason", out var reason);
        entity.Terminate(reason ?? "手动终止");

        // 提交终止审批
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_TERMINATE", ct);
        if (approvalType != null)
        {
            var result = await _approvalService.SubmitAsync(new SubmitApprovalRequest
            {
                ApprovalTypeId = approvalType.Id,
                Title = $"[合同终止] {entity.ContractNo}",
                Description = reason ?? "手动终止",
                TargetEntityId = entity.Id,
                TargetEntityType = "Contract"
            }, ct);

            if (result != null)
                entity.SetStatus("PendingApproval");
        }

        await _uow.CommitAsync(ct);
        return Ok(new { id, status = entity.StatusCode });
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.Suspend();
        await _uow.CommitAsync(ct);
        return Ok(new { id, status = entity.StatusCode });
    }

    [HttpPost("{id}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.Resume();
        await _uow.CommitAsync(ct);
        return Ok(new { id, status = entity.StatusCode });
    }

    // ===== 续签相关 API（新审批流程） =====

    /// <summary>续签预览：检查欠费、并发、展示继承配置</summary>
    [HttpGet("{id}/renewal/preview")]
    public async Task<IActionResult> RenewalPreview(Guid id, CancellationToken ct)
    {
        try
        {
            var preview = await _renewalService.PreviewAsync(id, ct);
            return Ok(preview);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>提交续签审批</summary>
    [HttpPost("{id}/renewal/submit")]
    public async Task<IActionResult> SubmitRenewal(Guid id, [FromBody] RBS.Application.DTOs.Contract.SubmitRenewalRequest request, CancellationToken ct)
    {
        request.ContractId = id;
        try
        {
            var userId = GetCurrentUserId();
            var result = await _renewalService.SubmitAsync(request, userId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>获取续签历史</summary>
    [HttpGet("{id}/renewal/history")]
    public async Task<IActionResult> GetRenewalHistory(Guid id, CancellationToken ct)
    {
        var history = await _renewalService.GetHistoryAsync(id, ct);
        return Ok(history);
    }

    /// <summary>获取续签链</summary>
    [HttpGet("{id}/renewal/chain")]
    public async Task<IActionResult> GetRenewalChain(Guid id, CancellationToken ct)
    {
        var chain = await _renewalService.GetRenewalChainAsync(id, ct);
        return Ok(chain);
    }

    /// <summary>获取合同允许的操作</summary>
    [HttpGet("{id}/allowed-operations")]
    public async Task<IActionResult> GetAllowedOperations(Guid id, CancellationToken ct)
    {
        try
        {
            var ops = await _renewalService.GetAllowedOperationsAsync(id, ct);
            return Ok(ops);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private Guid GetCurrentUserId()
    {
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var events = await _timelineService.GetTimelineAsync(id, ct);
        return Ok(events);
    }
}
