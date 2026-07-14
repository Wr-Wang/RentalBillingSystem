using System.Data;
using Dapper;
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
    private readonly IJournalGenerationService _journalGen;
    private readonly IReceivableGenerationService _receivableGen;
    private readonly IServiceProvider _serviceProvider;

    public ContractsController(IContractService contractService, IRenewalService renewalService,
        IContractDomainService contractDomainService, IApprovalService approvalService,
        IContractTimelineService timelineService, IUnitOfWork uow,
        IDbConnectionFactory db, ISqlLoader sql, ICurrentUserService currentUser,
        IJournalGenerationService journalGen,
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
        _journalGen = journalGen;
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
        var result2 = await _contractService.GetPagedListAsync(companyId.Value, page, pageSize, keyword, status, roomId, ct);
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
            using var conn2 = _db.CreateConnection(); conn2.Open();
            using var tx2 = conn2.BeginTransaction();
            try
            {
                foreach (var item in request.Items)
                {
                    var effDate = item.EffectiveDate ?? "";
                    if (string.IsNullOrEmpty(effDate)) continue;

                    var current = await conn2.QuerySingleOrDefaultAsync(
                        _sql.Get("Lease.Select.ContractFeeConfig.CurrentByContractAndFee"),
                        new { ContractId = id, FeeCodeId = item.FeeCodeId }, tx2);

                    var overlap = await conn2.QuerySingleAsync<int>(
                        _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
                        new { ContractId = id, FeeCodeId = item.FeeCodeId,
                            EffectiveDate = effDate, ExpiryDate = (string?)null,
                            ExcludeId = current != null ? (Guid)((dynamic)current).Id : (Guid?)null }, tx2);
                    if (overlap > 0)
                        throw new InvalidOperationException("费用项 " + item.FeeName + " 的生效日期与已有记录冲突");

                    var expiryDate = DateTime.Parse(effDate).AddDays(-1).ToString("yyyy-MM-dd");
                    if (current != null)
                    {
                        await conn2.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.ExpiryDate"),
                            new { Id = (Guid)((dynamic)current).Id, ExpiryDate = expiryDate }, tx2);
                        await conn2.ExecuteAsync(_sql.Get("Contract.Update.ContractFeeConfig.ExpireByCodeId"),
                            new { ExpiryDate = expiryDate, ContractId = id, FeeCodeId = item.FeeCodeId }, tx2);
                    }
                    await conn2.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
                        new { Id = Guid.NewGuid(), ContractId = id, FeeCodeId = item.FeeCodeId,
                            BillingMode = item.BillingMode ?? "FixedAmount", Amount = item.NewAmount,
                            EffectiveDate = effDate, CreatedBy = userId, Now = ChinaTime.Now }, tx2);
                }
                tx2.Commit();
            }
            catch (Exception ex)
            {
                tx2.Rollback();
                return BadRequest(new { error = "[Tx] " + ex.Message });
            }

            // 以下为 Commit 后的操作（独立 try，失败不影响调价）
            try
            {
                foreach (var item in request.Items)
                {
                    var effDate2 = item.EffectiveDate ?? "";
                    if (!string.IsNullOrEmpty(effDate2))
                        await InsertChangeHistoryAsync(_db.CreateConnection(), null, id, "FEE_ADJUST",
                            "费用调价", item.FeeName + ": " + item.OldAmount.ToString("F2") + " -> " + item.NewAmount.ToString("F2") + "，生效 " + effDate2,
                            item.OldAmount, item.NewAmount, effDate2, userId);
                }

                var currentMonth = DateOnly.FromDateTime(ChinaTime.Now).ToString("yyyy-MM");
                foreach (var item in request.Items)
                {
                    var effDate2 = item.EffectiveDate ?? "";
                    if (!string.IsNullOrEmpty(effDate2))
                    {
                        var effMonth = effDate2.Substring(0, 7);
                        if (string.Compare(effMonth, currentMonth, StringComparison.Ordinal) <= 0 ||
                            effMonth == DateOnly.FromDateTime(ChinaTime.Now).AddMonths(1).ToString("yyyy-MM"))
                        {
                            await _journalGen.GenerateSupplementaryAsync(
                                id, item.FeeCodeId, item.NewAmount, item.OldAmount,
                                effDate2, effMonth, ct);
                        }
                    }
                }
            }
            catch { }

            return Ok(new { message = "费用调价已直接执行" });
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
            // ★ Dapper DynamicParameters 无法从 null DateTime? 推断 DbType → 转为 object
            object effectiveDateParam = request.EffectiveDate.HasValue
                ? (object)request.EffectiveDate.Value.Date
                : DBNull.Value;

            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Insert.ApprovalBizData.FeeAdjust"),
                new { Id = bizDataId, ContractId = id, ContractNo = contract.ContractNo,
                    CompanyId = contract.CompanyId, EffectiveDate = effectiveDateParam,
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
                        EffectiveDate = item.EffectiveDate ?? "",
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
        public string? EffectiveDate { get; set; }
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
            return BadRequest(new { code = "CONTRACT_ALREADY_TERMINATED", message = "合同已终止" });

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
            try { using var conn = _db.CreateConnection(); conn.Open();
                await InsertChangeHistoryAsync(conn, null, id, "TERMINATE",
                    "合同终止", request.Reason ?? "", null, null,
                    request.ActualEndDate?.ToString("yyyy-MM-dd"), userId); } catch { }
            return Ok(new { message = "合同已终止" });
        }

        // 有审批 → 走审批流
        var conflict = await EnsureNoPendingForContractAsync(id, ct);
        if (conflict != null) return conflict;

        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var bizDataId = Guid.NewGuid();
            object actualEndDateParam = request.ActualEndDate.HasValue
                ? (object)request.ActualEndDate.Value.Date
                : DBNull.Value;

            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Insert.ApprovalBizData.Terminate"),
                new { Id = bizDataId, ContractId = id, ContractNo = entity.ContractNo,
                    CompanyId = entity.CompanyId,
                    TerminateType = request.TerminateType ?? "EARLY",
                    ActualEndDate = actualEndDateParam,
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


    public class SupplementaryFeeRequest
    {
        public Guid FeeCodeId { get; set; }
        public decimal Amount { get; set; }
        public string EffectiveDate { get; set; } = "";
        public string BillingMode { get; set; } = "FixedAmount";
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
    [HttpPost("{id}/suspendpreview")]
    public async Task<IActionResult> SuspendPreview(Guid id, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();

        using var conn = _db.CreateConnection(); conn.Open();
        var pendingAmount = await conn.QuerySingleAsync<decimal>(
            "SELECT ISNULL(SUM(Amount - Received), 0) FROM ReceivablePlans WHERE ContractId=@C AND Status IN ('Pending','Partial','Overdue')",
            new { C = id });

        var nextPeriods = new List<string>();
        var dateNow = DateOnly.FromDateTime(ChinaTime.Now);
        for (int m = 0; m < 3; m++)
        {
            var dt = dateNow.AddMonths(m);
            if (contract.ShouldGenerateReceivableFor($"{dt.Year:D4}-{dt.Month:D2}"))
                nextPeriods.Add($"{dt.Year:D4}-{dt.Month:D2}");
        }

        return Ok(new
        {
            contractNo = contract.ContractNo,
            receivableImpact = new
            {
                totalPendingAmount = pendingAmount,
                pendingPeriods = new List<string>(),
                frozenPeriods = nextPeriods,
                frozenAmount = 0
            },
            warnings = pendingAmount > 0 ? new[] { $"合同有未结清应收 ¥{pendingAmount:N2}，暂停后仍需催收" } : Array.Empty<string>()
        });
    }

    [HttpPost("{id}/suspendsubmit")]
    public async Task<IActionResult> SuspendSubmit(Guid id, [FromBody] SuspendRequest request, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { message = "暂停原因不能为空" });

        var userId = GetCurrentUserId();
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_SUSPEND", ct);
        if (approvalType == null)
        {
            // 无审批直接执行
            entity.Suspend();
            await _uow.CommitAsync(ct);
            await InsertChangeHistoryAsync(_db.CreateConnection(), null, id, "SUSPEND", "合同暂停",
                request.Reason ?? "", null, null, ChinaTime.Now.ToString("yyyy-MM-dd"), userId);
            return Ok(new { id, status = entity.Status, message = "合同已暂停" });
        }

        // 有审批走审批流
        var bizDataId = Guid.NewGuid();
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Insert.ApprovalBizData.Suspend"),
            new { Id = bizDataId, ContractId = id, ContractNo = entity.ContractNo,
                CompanyId = entity.CompanyId, Reason = request.Reason ?? "",
                CreatedBy = userId, CreatedAt = ChinaTime.Now });

        var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
        {
            ApprovalTypeId = approvalType.Id, Title = $"[合同暂停] {entity.ContractNo}",
            Description = request.Reason, TargetEntityId = id,
            TargetEntityType = "ContractSuspend"
        }, ct);

        await _uow.ExecuteSqlRawAsync(_sql.Get("Approval.Update.ApprovalRequest.SetContractId"),
            new { Id = approvalResult.Id, ContractId = id }, ct);
        return Ok(new { status = "PendingApproval", approvalRequestId = approvalResult.Id, message = "暂停申请已提交审批" });
    }

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

        await InsertChangeHistoryAsync(_db.CreateConnection(), null, id, "SUSPEND",
            "合同暂停", "合同暂停，原因 " + (request.Reason ?? ""), null, null,
            ChinaTime.Now.ToString("yyyy-MM-dd"), userId);
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
        await InsertChangeHistoryAsync(_db.CreateConnection(), null, id, "RESUME",
            "合同恢复", "合同已恢复", null, null,
            ChinaTime.Now.ToString("yyyy-MM-dd"), GetCurrentUserId());
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
    // 补充收费
    // ===================================================================
    [HttpPost("{id}/supplementaryfee/preview")]
    public async Task<IActionResult> PreviewSupplementaryFee(Guid id, [FromBody] SupplementaryFeePreviewRequest request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();
        var effDate = DateOnly.Parse(request.EffectiveDate);
        if (effDate < contract.StartDate)
            return BadRequest(new { code = "EFF_DATE_BEFORE_CONTRACT_START", message = "生效日期不能早于合同起租日期" });
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        var items = new List<object>();
        decimal total = 0;
        for (var m = new DateOnly(effDate.Year, effDate.Month, 1); m < new DateOnly(today.Year, today.Month, 1); m = m.AddMonths(1))
        {
            var daysInMonth = DateTime.DaysInMonth(m.Year, m.Month);
            var monthEnd = new DateOnly(m.Year, m.Month, daysInMonth);
            var overlapStart = effDate > m ? effDate : m;
            var overlapDays = monthEnd.DayNumber - overlapStart.DayNumber + 1;
            var prorated = Math.Round(request.Amount / daysInMonth * overlapDays, 2);
            items.Add(new { period = m.ToString("yyyy-MM"), daysInMonth, coveredDays = overlapDays, proratedAmount = prorated });
            total += prorated;
        }
        return Ok(new { feeName = "", amount = request.Amount, effectiveDate = request.EffectiveDate,
            items, totalAmount = Math.Round(total, 2),
            accountingImpact = new { } });
    }

    [HttpPost("{id}/supplementaryfee/request")]
    public async Task<IActionResult> SubmitSupplementaryFee(Guid id, [FromBody] SupplementaryFeePreviewRequest request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();
        var effDate = DateOnly.Parse(request.EffectiveDate);
        if (effDate < contract.StartDate)
            return BadRequest(new { code = "EFF_DATE_BEFORE_CONTRACT_START" });
        var userId = GetCurrentUserId();
        var today = ChinaTime.Now;
        var currentMonth = new DateOnly(today.Year, today.Month, 1);
        var startMonth = new DateOnly(effDate.Year, effDate.Month, 1);
        var periodFrom = startMonth.ToString("yyyy-MM");
        var periodTo = currentMonth.AddMonths(-1).ToString("yyyy-MM");
        if (string.Compare(periodFrom, periodTo, StringComparison.Ordinal) > 0)
            return BadRequest(new { message = "生效日期在当前月之后，无需追溯" });

        var suppReq = new RBS.Core.Entities.Contract.SupplementaryFeeRequest(id, request.FeeCodeId, request.Amount, request.EffectiveDate, periodFrom, periodTo, contract.CompanyId);
        suppReq.SetCreated(userId, today, null, null);
        await _uow.SupplementaryFeeRequests.AddAsync(suppReq, ct);

        for (var m = startMonth; m < currentMonth; m = m.AddMonths(1))
        {
            var daysInMonth = DateTime.DaysInMonth(m.Year, m.Month);
            var monthEnd = new DateOnly(m.Year, m.Month, daysInMonth);
            var overlapStart = effDate > m ? effDate : m;
            var overlapDays = monthEnd.DayNumber - overlapStart.DayNumber + 1;
            var prorated = Math.Round(request.Amount / daysInMonth * overlapDays, 2);
            var item = new RBS.Core.Entities.Contract.SupplementaryFeeRequestItem(suppReq.Id, m.ToString("yyyy-MM"), prorated, daysInMonth, overlapDays);
            await _uow.SupplementaryFeeRequestItems.AddAsync(item, ct);
        }
        await _uow.CommitAsync(ct);

        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_SUPPLEMENTARY_FEE", ct);
        if (approvalType == null)
        {
            await ExecuteSupplementaryFeeAsync(suppReq.Id, ct);
            return Ok(new { status = "Completed" });
        }
        suppReq.Submit(); await _uow.CommitAsync(ct);
        var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
        {
            ApprovalTypeId = approvalType.Id, Title = $"[补充收费] {contract.ContractNo}",
            Description = $"新增 {request.Amount:N2}/月，生效 {request.EffectiveDate}",
            TargetEntityId = suppReq.Id, TargetEntityType = "SupplementaryFee"
        }, ct);
        return Ok(new { status = "PendingApproval", requestId = suppReq.Id, approvalRequestId = approvalResult.Id });
    }

    private async Task ExecuteSupplementaryFeeAsync(Guid requestId, CancellationToken ct)
    {
        var request = await _uow.SupplementaryFeeRequests.GetByIdAsync(requestId, ct);
        if (request == null) return;
        var allItems = await _uow.SupplementaryFeeRequestItems.GetAllAsync(ct);
        var items = allItems.Where(i => i.RequestId == requestId).ToList();
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // ★ 校验新生效日不与其他 FeeConfig 区间交叉
            var overlap = await conn.QuerySingleAsync<int>(
                _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
                new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                    EffectiveDate = request.EffectiveDate, ExpiryDate = (string?)null,
                    ExcludeId = (Guid?)null }, tx);
            if (overlap > 0)
                throw new InvalidOperationException("费用配置生效日期与已有记录存在交叉，请调整生效日期");

            // ★ 创建 ContractFeeConfig（确保后续 BillJob 持续出账）
            var configId = Guid.NewGuid();
            await conn.ExecuteAsync(
                _sql.Get("Lease.Insert.ContractFeeConfig.Default"),
                new { Id = configId, ContractId = request.ContractId,
                    FeeCodeId = request.FeeCodeId, BillingMode = request.BillingMode ?? "FixedAmount",
                    Amount = request.Amount, Unit = (string?)null, UnitPrice = (decimal?)null,
                    EffectiveDate = request.EffectiveDate,
                    CreatedBy = request.CreatedBy, Now = ChinaTime.Now }, tx);

            var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
                _sql.Get("Accounting.Select.Subject.ByCodes"), tx)).ToDictionary(r => r.Code, r => r.Id);
            var receivableId = subjects.GetValueOrDefault("1122", Guid.Empty);
            var revenueId = subjects.GetValueOrDefault("6001", subjects.GetValueOrDefault("6051", Guid.Empty));
            foreach (var item in items)
            {
                var exists = await conn.QuerySingleAsync<int>(_sql.Get("Billing.Select.ReceivablePlan.ExistsByKey"),
                    new { C = request.ContractId, F = request.FeeCodeId, P = item.Period }, tx);
                if (exists > 0) continue;
                var planId = Guid.NewGuid();
                await conn.ExecuteAsync(_sql.Get("Billing.Insert.ReceivablePlan.Default"),
                    new { Id = planId, CId = request.ContractId, FId = request.FeeCodeId, P = item.Period,
                        Amt = item.ProratedAmount, Due = DateOnly.FromDateTime(ChinaTime.Now), CBy = Guid.Empty }, tx);
                var vid = Guid.NewGuid();
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.Voucher.BillJob"),
                    new { Id = vid, No = "SUP-" + Guid.NewGuid().ToString("N")[..24], Date = DateOnly.FromDateTime(ChinaTime.Now),
                        Desc = "补充收费", SrcId = request.ContractId, Type = "SupplementaryFee", CId = request.ContractId, CBy = Guid.Empty }, tx);
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = vid, SId = receivableId, Dir = "Debit",
                        Amt = item.ProratedAmount, Sum = "补充收费 " + item.Period, CBy = Guid.Empty }, tx);
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = vid, SId = revenueId, Dir = "Credit",
                        Amt = item.ProratedAmount, Sum = "补充收费 " + item.Period, CBy = Guid.Empty }, tx);
            }
            await conn.ExecuteAsync(_sql.Get("SupplementaryFee.Update.Request.Complete"),
                new { Id = requestId, FeeConfigId = configId }, tx);
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public class SupplementaryFeePreviewRequest
    {
        public Guid FeeCodeId { get; set; }
        public decimal Amount { get; set; }
        public string EffectiveDate { get; set; } = "";
        public string BillingMode { get; set; } = "FixedAmount";
    }

    [HttpPost("{id}/supplementaryfee")]
    public async Task<IActionResult> AddSupplementaryFee(Guid id, [FromBody] SupplementaryFeeRequest request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();

        var effDate = DateOnly.Parse(request.EffectiveDate);
        if (effDate < contract.StartDate)
            return BadRequest(new { code = "EFF_DATE_BEFORE_CONTRACT_START", message = "生效日期不能早于合同起租日期" });

        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            var configId = Guid.NewGuid();
            await conn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
                new { Id = configId, ContractId = id, FeeCodeId = request.FeeCodeId,
                    BillingMode = request.BillingMode ?? "FixedAmount", Amount = request.Amount,
                    EffectiveDate = request.EffectiveDate,
                    CreatedBy = GetCurrentUserId(), Now = ChinaTime.Now });

            var today = ChinaTime.Now;
            var currentMonth = new DateOnly(today.Year, today.Month, 1);
            var startMonth = new DateOnly(effDate.Year, effDate.Month, 1);
            var subjects = await LoadSubjectsAsync(ct);
            var results = new List<object>();

            for (var m = startMonth; m < currentMonth; m = m.AddMonths(1))
            {
                var daysInMonth = DateTime.DaysInMonth(m.Year, m.Month);
                var monthStart = m;
                var monthEnd = new DateOnly(m.Year, m.Month, daysInMonth);
                var overlapStart = effDate > monthStart ? effDate : monthStart;
                var overlapDays = monthEnd.DayNumber - overlapStart.DayNumber + 1;
                var prorated = Math.Round(request.Amount / daysInMonth * overlapDays, 2);

                var planId = Guid.NewGuid();
                await conn.ExecuteAsync(_sql.Get("Billing.Insert.ReceivablePlan.Default"),
                    new { Id = planId, CId = id, FId = request.FeeCodeId,
                        P = m.ToString("yyyy-MM"), Amt = prorated,
                        Due = DateOnly.FromDateTime(today), CBy = Guid.Empty }, tx);

                var vid = Guid.NewGuid();
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.Voucher.BillJob"),
                    new { Id = vid, No = $"SUP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 32),
                        Date = DateOnly.FromDateTime(today), Desc = $"补充收费 {request.EffectiveDate}",
                        SrcId = id, Type = "SupplementaryFee", CId = id, CBy = Guid.Empty }, tx);
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = vid, SId = subjects["1122"],
                        Dir = "Debit", Amt = prorated, Sum = $"补充收费 {m:yyyy-MM}", CBy = Guid.Empty }, tx);
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = vid, SId = subjects["6001"],
                        Dir = "Credit", Amt = prorated, Sum = $"补充收费 {m:yyyy-MM}", CBy = Guid.Empty }, tx);

                results.Add(new { period = m.ToString("yyyy-MM"), amount = prorated });
            }

            tx.Commit();
            var totalAmount = results.Sum(r => (decimal)((dynamic)r).amount);
            await InsertChangeHistoryAsync(_db.CreateConnection(), null, id, "SUPPLEMENTARY_FEE",
                "补充收费", $"新增费用 {request.Amount:F2}/月，生效 {request.EffectiveDate}，追溯 {results.Count} 个月",
                0, request.Amount, request.EffectiveDate, GetCurrentUserId());
            return Ok(new { createdCount = results.Count, totalAmount, items = results });
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // ===================================================================
    // 变更历史
    // ===================================================================
    [HttpGet("{id}/changes")]
    public async Task<IActionResult> GetChanges(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync(_sql.Get("Contract.Select.ChangeHistory.ByContract"),
            new { ContractId = id });
        return Ok(rows);
    }

    // ===================================================================
    // 合同租客管理
    // ===================================================================
    [HttpGet("{id}/tenants")]
    public async Task<IActionResult> GetContractTenants(Guid id, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync(_sql.Get("Lease.Select.ContractTenant.DetailByContract"), new { ContractId = id });
        var list = rows.Select(r => new {
            tenantId = (Guid)r.TenantId, tenantName = (string)r.TenantName,
            tenantPhone = (string?)r.TenantPhone, idCard = (string?)r.IdCard,
            email = (string?)r.Email, wechat = (string?)r.Wechat, isPrimary = (bool)r.IsPrimary
        }).ToList();
        return Ok(new { contractId = id, tenants = list });
    }

    [HttpPost("{id}/tenants")]
    public async Task<IActionResult> AddContractTenant(Guid id, [FromBody] AddContractTenantDto request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();
        if (request.TenantId == null && string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "必须指定租客" });
        var userId = GetCurrentUserId();
        Guid tenantId;
        if (request.TenantId.HasValue) { tenantId = request.TenantId.Value; }
        else
        {
            var newTenant = new Tenant(request.Name!, contract.CompanyId);
            newTenant.SetPhone(request.Phone); newTenant.SetIdCard(request.IdCard);
            newTenant.SetCreated(userId, ChinaTime.Now, null, null);
            await _uow.Tenants.AddAsync(newTenant, ct); await _uow.CommitAsync(ct);
            tenantId = newTenant.Id;
        }
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_TENANT_CHANGE", ct);
        if (approvalType == null)
        {
            contract.AddTenant(tenantId, request.IsPrimary); await _uow.CommitAsync(ct);
            try { using var c = _db.CreateConnection(); c.Open();
                var n = await c.QuerySingleOrDefaultAsync<string>("SELECT Name FROM Tenants WHERE Id=@Id", new { Id = tenantId });
                await InsertChangeHistoryAsync(c, null, id, "TENANT_ADD", "添加租客", $"添加租客: {n}", null, null, null, userId); } catch { }
            return Ok(new { status = "Completed", message = "租客已添加" });
        }
        var bid = Guid.NewGuid();
        await _uow.ExecuteSqlRawAsync(_sql.Get("Contract.Insert.ApprovalBizData.TenantChange"),
            new { Id = bid, ApprovalRequestId = (Guid?)null, ContractId = id,
                ContractNo = contract.ContractNo ?? "", CompanyId = contract.CompanyId,
                ChangeType = "TENANT_ADD", OldAmount = 0m, Reason = tenantId.ToString(),
                CreatedBy = userId, CreatedAt = ChinaTime.Now });
        var ar = await _approvalService.SubmitAsync(new SubmitApprovalRequest {
            ApprovalTypeId = approvalType.Id, Title = $"[添加租客] {contract.ContractNo}",
            Description = "添加租客", TargetEntityId = id, TargetEntityType = "ContractTenantChange" }, ct);
        await _uow.ExecuteSqlRawAsync(_sql.Get("Approval.Update.Request.SetContractId"), new { Id = ar.Id, ContractId = id });
        await _uow.CommitAsync(ct);
        return Ok(new { status = "PendingApproval", approvalRequestId = ar.Id });
    }

    [HttpDelete("{id}/tenants/{tenantId}")]
    public async Task<IActionResult> RemoveContractTenant(Guid id, Guid tenantId, [FromBody] RemoveContractTenantDto request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { message = "解绑原因不能为空" });
        var userId = GetCurrentUserId();
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_TENANT_CHANGE", ct);
        if (approvalType == null)
        {
            contract.RemoveTenant(tenantId); await _uow.CommitAsync(ct);
            try { using var c = _db.CreateConnection(); c.Open();
                await InsertChangeHistoryAsync(c, null, id, "TENANT_REMOVE", "移除租客", $"移除租客: {request.Reason}", null, null, null, userId); } catch { }
            return Ok(new { status = "Completed", message = "租客已解绑" });
        }
        var bid = Guid.NewGuid();
        await _uow.ExecuteSqlRawAsync(_sql.Get("Contract.Insert.ApprovalBizData.TenantChange"),
            new { Id = bid, ApprovalRequestId = (Guid?)null, ContractId = id,
                ContractNo = contract.ContractNo ?? "", CompanyId = contract.CompanyId,
                ChangeType = "TENANT_REMOVE", OldAmount = 0m, Reason = tenantId.ToString(),
                CreatedBy = userId, CreatedAt = ChinaTime.Now });
        var ar = await _approvalService.SubmitAsync(new SubmitApprovalRequest {
            ApprovalTypeId = approvalType.Id, Title = $"[移除租客] {contract.ContractNo}",
            Description = request.Reason, TargetEntityId = id, TargetEntityType = "ContractTenantChange" }, ct);
        await _uow.ExecuteSqlRawAsync(_sql.Get("Approval.Update.Request.SetContractId"), new { Id = ar.Id, ContractId = id });
        await _uow.CommitAsync(ct);
        return Ok(new { status = "PendingApproval", approvalRequestId = ar.Id });
    }

    [HttpPut("{id}/tenants/{tenantId}/primary")]
    public async Task<IActionResult> SetPrimaryTenant(Guid id, Guid tenantId, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct);
        if (contract == null) return NotFound();
        var userId = GetCurrentUserId();
        await _uow.ExecuteSqlRawAsync(_sql.Get("Lease.Update.ContractTenant.SetPrimary"), new { ContractId = id, TenantId = tenantId });
        try { using var c = _db.CreateConnection(); c.Open();
            var n = await c.QuerySingleOrDefaultAsync<string>("SELECT Name FROM Tenants WHERE Id=@Id", new { Id = tenantId });
            await InsertChangeHistoryAsync(c, null, id, "TENANT_PRIMARY", "设置主租户", $"设置主租户: {n}", null, null, null, userId); } catch { }
        return Ok(new { message = "主租户已更新" });
    }

    public class AddContractTenantDto
    {
        public Guid? TenantId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? IdCard { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class RemoveContractTenantDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    // ===================================================================
    // 并发守卫
    // ===================================================================
    private async Task<IActionResult?> EnsureNoPendingForContractAsync(Guid contractId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var hasPending = await Dapper.SqlMapper.QuerySingleAsync<int>(conn,
            _sql.Get("Approval.Select.Request.PendingByContractId"),
            new { Id = contractId });
        if (hasPending > 0)
            return Conflict(new { code = "PENDING_APPROVAL_EXISTS", message = "该合同存在待审批的申请，请处理完成后再提交" });
        return null;
    }

    private async Task<Dictionary<string, Guid>> LoadSubjectsAsync(CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes"));
        return rows.ToDictionary(r => r.Code, r => r.Id);
    }

    private async Task InsertChangeHistoryAsync(IDbConnection conn, IDbTransaction? tx,
        Guid contractId, string changeType, string title, string detail,
        decimal? oldValue, decimal? newValue, string? effectiveDate, Guid? operatorId, string? operatorName = null)
    {
        if (string.IsNullOrEmpty(operatorName) && operatorId.HasValue)
        {
            try { operatorName = await conn.QuerySingleOrDefaultAsync<string>(
                "SELECT DisplayName FROM Users WHERE Id=@Id", new { Id = operatorId }, tx); } catch { }
        }
        await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
            new { Id = Guid.NewGuid(), ContractId = contractId, ChangeType = changeType,
                Title = title, Detail = detail, OldValue = oldValue, NewValue = newValue,
                EffectiveDate = effectiveDate, OperatorId = operatorId, OperatorName = operatorName ?? "" }, tx);
    }

    private Guid GetCurrentUserId()
    {
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }
}
