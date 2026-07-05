using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
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

    public ContractsController(IContractService contractService, IRenewalService renewalService,
        IContractDomainService contractDomainService, IApprovalService approvalService,
        IContractTimelineService timelineService, IUnitOfWork uow,
        IDbConnectionFactory db, ISqlLoader sql, ICurrentUserService currentUser)
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

    // ===================================================================
    // 租金调整（★ v3 改造 — 审批驱动）
    // ===================================================================
    [HttpPost("{id}/rentadjust")]
    public async Task<IActionResult> RentAdjust(Guid id, [FromBody] RentAdjustRequest request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();

        var userId = GetCurrentUserId();

        // 并发守卫
        await EnsureNoPendingForContractAsync(id, ct);

        // 找审批类型
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_MODIFY", ct);
        if (approvalType == null)
            return BadRequest(new { error = "未配置合同租金调整审批类型，请联系管理员" });

        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // 1. 写业务数据
            var bizDataId = Guid.NewGuid();
            var effectiveDate = request.EffectiveDate.HasValue
                ? DateOnly.FromDateTime(request.EffectiveDate.Value)
                : (DateOnly?)null;
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Insert.ApprovalBizData.RentAdjust"),
                new { Id = bizDataId, ContractId = id, ContractNo = contract.ContractNo,
                    CompanyId = contract.CompanyId, EffectiveDate = effectiveDate,
                    OldAmount = contract.RentAmount.Amount, NewAmount = request.NewAmount,
                    Reason = request.Reason ?? "", CreatedBy = userId, CreatedAt = ChinaTime.Now });

            // 2. 提交审批
            var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
            {
                ApprovalTypeId = approvalType.Id,
                Title = $"合同租金调整 - {contract.ContractNo}",
                Description = "",
                TargetEntityId = id,
                TargetEntityType = "ContractRent"
            }, ct);

            // 3. 回写 ApprovalRequestId + ContractId
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Update.ApprovalBizData.SetApprovalRequestId"),
                new { Id = bizDataId, ApprovalRequestId = approvalResult.Id });
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Update.ApprovalRequest.SetContractId"),
                new { Id = approvalResult.Id, ContractId = id });

            await tx.CommitAsync(ct);
            return Ok(new { id = approvalResult.Id, status = approvalResult.Status, message = "租金调整申请已提交审批" });
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public class RentAdjustRequest
    {
        public decimal NewAmount { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? Reason { get; set; }
    }

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
        await EnsureNoPendingForContractAsync(id, ct);

        // 找审批类型
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_FEE_CHANGE", ct);

        // 无审批配置 → 0 级直接执行
        if (approvalType == null)
        {
            var effectiveDate = request.EffectiveDate.HasValue
                ? DateOnly.FromDateTime(request.EffectiveDate.Value)
                : (DateOnly?)null;
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Insert.ApprovalBizData.FeeAdjust"),
                new { Id = Guid.NewGuid(), ContractId = id, ContractNo = contract.ContractNo,
                    CompanyId = contract.CompanyId, EffectiveDate = effectiveDate,
                    Reason = request.Reason ?? "", CreatedBy = userId, CreatedAt = ChinaTime.Now,
                    ApprovalRequestId = (Guid?)null });
            return Ok(new { message = "费用调价已直接执行（无审批配置）" });
        }

        // 1. 先创建审批，拿到 ApprovalRequestId
        var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
        {
            ApprovalTypeId = approvalType.Id,
            Title = $"合同费用调价 - {contract.ContractNo}",
            Description = "",
            TargetEntityId = id,
            TargetEntityType = "ContractFeeAdjust"
        }, ct);

        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // 2. 写业务数据（带 ApprovalRequestId）
            var bizDataId = Guid.NewGuid();
            var effectiveDate = request.EffectiveDate.HasValue
                ? DateOnly.FromDateTime(request.EffectiveDate.Value)
                : (DateOnly?)null;

            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Insert.ApprovalBizData.FeeAdjust"),
                new { Id = bizDataId, ContractId = id, ContractNo = contract.ContractNo,
                    CompanyId = contract.CompanyId, EffectiveDate = effectiveDate,
                    Reason = request.Reason ?? "", CreatedBy = userId, CreatedAt = ChinaTime.Now,
                    ApprovalRequestId = approvalResult.Id });

            // 3. 写费用项明细（带 ApprovalRequestId）
            foreach (var item in request.Items)
            {
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Contract.Insert.ApprovalFeeItem.ForFeeAdjust"),
                    new { Id = Guid.NewGuid(), ContractId = id, item.FeeCodeId, item.FeeName,
                        OldAmount = item.OldAmount, NewAmount = item.NewAmount,
                        BillingMode = item.BillingMode, Unit = item.Unit,
                        CreatedBy = userId, CreatedAt = ChinaTime.Now,
                        ApprovalRequestId = approvalResult.Id });
            }

            // 4. 回写 ContractId
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Update.ApprovalRequest.SetContractId"),
                new { Id = approvalResult.Id, ContractId = id });

            await tx.CommitAsync(ct);
            return Ok(new { id = approvalResult.Id, status = approvalResult.Status, message = "费用调价申请已提交审批" });
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public class FeeAdjustRequest
    {
        public DateTime? EffectiveDate { get; set; }
        public string? Reason { get; set; }
        public List<FeeAdjustItem> Items { get; set; } = new();
    }

    public class FeeAdjustItem
    {
        public Guid FeeCodeId { get; set; }
        public string FeeName { get; set; } = "";
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public string BillingMode { get; set; } = "FixedAmount";
        public string? Unit { get; set; }
    }

    // ===================================================================
    // 终止（★ v3 修复 — 不再提前 Terminate，改为审批驱动）
    // ===================================================================
    [HttpPost("{id}/terminate")]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] TerminateRequest request, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();

        if (entity.Status == "Terminated")
            return BadRequest(new { error = "合同已终止" });

        var userId = GetCurrentUserId();

        // ★ v3 修复：不再提前调 entity.Terminate()
        // 先检查是否有审批类型
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_TERMINATE", ct);
        if (approvalType == null)
        {
            // 无审批配置 → 0 级直接执行
            await _contractDomainService.ExecuteContractTerminationAsync(
                id,
                request.ActualEndDate.HasValue ? DateOnly.FromDateTime(request.ActualEndDate.Value) : null,
                request.DepositReturn ?? "FULL",
                request.Reason ?? "合同终止",
                userId, ct);
            return Ok(new { message = "合同已终止" });
        }

        // 有审批 → 走审批流
        await EnsureNoPendingForContractAsync(id, ct);

        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var bizDataId = Guid.NewGuid();
            var effectiveDate = request.ActualEndDate.HasValue
                ? DateOnly.FromDateTime(request.ActualEndDate.Value)
                : (DateOnly?)null;

            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Insert.ApprovalBizData.Terminate"),
                new { Id = bizDataId, ContractId = id, ContractNo = entity.ContractNo,
                    CompanyId = entity.CompanyId,
                    TerminateType = request.TerminateType ?? "EARLY",
                    ActualEndDate = effectiveDate,
                    DepositReturn = request.DepositReturn ?? "FULL",
                    Reason = request.Reason ?? "", CreatedBy = userId, CreatedAt = ChinaTime.Now });

            var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
            {
                ApprovalTypeId = approvalType.Id,
                Title = $"[合同终止] {entity.ContractNo}",
                Description = request.Reason ?? "手动终止",
                TargetEntityId = id,
                TargetEntityType = "ContractTerminate"
            }, ct);

            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Update.ApprovalBizData.SetApprovalRequestId"),
                new { Id = bizDataId, ApprovalRequestId = approvalResult.Id });
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Update.ApprovalRequest.SetContractId"),
                new { Id = approvalResult.Id, ContractId = id });

            await tx.CommitAsync(ct);
            return Ok(new { id = approvalResult.Id, status = approvalResult.Status, message = "终止申请已提交审批" });
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public class TerminateRequest
    {
        public string? TerminateType { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string? DepositReturn { get; set; }
        public string? Reason { get; set; }
    }

    // ===================================================================
    // 暂停（★ v3 补充：写 ApprovalBizData）
    // ===================================================================
    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendRequest request, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();

        entity.Suspend();
        await _uow.CommitAsync(ct);

        // ★ 写业务数据（用于历史追溯）
        var userId = GetCurrentUserId();
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Insert.ApprovalBizData.Suspend"),
            new { Id = Guid.NewGuid(), ContractId = id, ContractNo = entity.ContractNo,
                CompanyId = entity.CompanyId, Reason = request.Reason ?? "", CreatedBy = userId, CreatedAt = ChinaTime.Now });

        return Ok(new { id, status = entity.Status });
    }

    public class SuspendRequest
    {
        public string? Reason { get; set; }
    }

    // ===================================================================
    // 恢复
    // ===================================================================
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.Resume(); // ★ v3 修复：内部已写入 ResumedAt + 触发 ContractResumedEvent
        await _uow.CommitAsync(ct);
        return Ok(new { id, status = entity.Status });
    }

    // ===== 续签相关 API（新审批流程） =====
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

    [HttpGet("{id}/renewal/history")]
    public async Task<IActionResult> GetRenewalHistory(Guid id, CancellationToken ct)
    {
        var history = await _renewalService.GetHistoryAsync(id, ct);
        return Ok(history);
    }

    [HttpGet("{id}/renewal/chain")]
    public async Task<IActionResult> GetRenewalChain(Guid id, CancellationToken ct)
    {
        var chain = await _renewalService.GetRenewalChainAsync(id, ct);
        return Ok(chain);
    }

    [HttpGet("{id}/renewal/lastrejected")]
    public async Task<IActionResult> GetLastRejectedRenewal(Guid id, CancellationToken ct)
    {
        var result = await _renewalService.GetLastRejectedAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/allowedoperations")]
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

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var events = await _timelineService.GetTimelineAsync(id, ct);
        return Ok(events);
    }

    // ===================================================================
    // 并发守卫
    // ===================================================================
    private async Task EnsureNoPendingForContractAsync(Guid contractId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var hasPending = await Dapper.SqlMapper.QuerySingleAsync<int>(conn,
            _sql.Get("Approval.Select.Request.PendingByContractId"),
            new { Id = contractId });
        if (hasPending > 0)
            throw new InvalidOperationException("该合同存在待审批的申请，请处理完成后再提交");
    }

    private Guid GetCurrentUserId()
    {
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }
}
