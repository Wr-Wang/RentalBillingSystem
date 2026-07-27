using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractFeeConfigsController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IApprovalService _approvalService;
    private readonly IBillingDomainService _billingDomain;

    public ContractFeeConfigsController(IDbConnectionFactory db, ISqlLoader sql, ICurrentUserService currentUser, IUnitOfWork uow, IApprovalService approvalService, IBillingDomainService billingDomain)
    {
        _db = db;
        _sql = sql;
        _currentUser = currentUser;
        _uow = uow;
        _approvalService = approvalService;
        _billingDomain = billingDomain;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        if (contractId == null) return Ok(new List<object>());
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync(_sql.Get("Lease.Select.ContractFeeConfig.ByContractId"),
            new { ContractId = contractId.Value });
        var list = rows.Select(r => MapConfig(r)).ToList();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeeConfigRequest request, CancellationToken ct)
    {
        try
        {
        // 校验合同状态：Active 合同必须走审批
        var contract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct);
        if (contract == null) return NotFound(new { message = "合同不存在" });

        // 校验生效日期在合同起止日期范围内
        var effDate = DateTime.Parse(request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"));
        Contract.ValidateFeeEffectiveDate(effDate, contract.StartDate, contract.EndDate);

        if (contract.Status == "Active")
        {
            var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_FEE_CHANGE", ct);
            if (approvalType != null)
            {
                var userId = _currentUser.UserId;

                // 查 FeeCode 获取真实名称和 ChargeType
                var feeCode = await _uow.FeeCodes.GetByIdAsync(request.FeeCodeId, ct);
                var feeName = feeCode?.Name ?? "";
                var chargeType = request.ChargeType ?? feeCode?.ChargeType ?? "Recurring";
                var chargeTypeLabel = chargeType == "OneTime" ? "一次性" : "周期性";

                ApprovalRequestDto approvalResult;
                try
                {
                    approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
                    {
                        ApprovalTypeId = approvalType.Id,
                        Title = $"[添加{chargeTypeLabel}费用] {contract.ContractNo}",
                        Description = $"添加 {feeName} ¥{request.Amount}",
                        TargetEntityId = request.ContractId,
                        TargetEntityType = "ContractFeeAdd"
                    }, ct);
                }
                catch (InvalidOperationException ex)
                {
                    return Conflict(new { code = "PENDING_APPROVAL_EXISTS", message = ex.Message });
                }

                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Contract.Insert.ApprovalFeeItem.ForFeeAdjust"),
                    new
                    {
                        Id = Guid.NewGuid(), ApprovalRequestId = approvalResult.Id,
                        ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                        FeeName = feeName, OldAmount = 0m, NewAmount = request.Amount,
                        BillingMode = request.BillingMode ?? "FixedAmount",
                        Unit = request.Unit, EffectiveDate = request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                        CreatedBy = userId, CreatedAt = ChinaTime.Now
                    }, ct);

                // 插入 ApprovalBizData（修复阻断 Bug）
                var bizDataId = Guid.NewGuid();
                object effectiveDateParam = request.EffectiveDate != null
                    ? (object)DateTime.Parse(request.EffectiveDate)
                    : DBNull.Value;
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Contract.Insert.ApprovalBizData.FeeAdjust"),
                    new
                    {
                        Id = bizDataId,
                        ApprovalRequestId = approvalResult.Id,
                        ContractId = request.ContractId,
                        ContractNo = contract.ContractNo,
                        CompanyId = contract.CompanyId,
                        EffectiveDate = effectiveDateParam,
                        Reason = $"添加{chargeTypeLabel}费用：{feeName}",
                        CreatedBy = userId,
                        CreatedAt = ChinaTime.Now
                    }, ct);

                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Contract.Update.ApprovalRequest.SetContractId"),
                    new { Id = approvalResult.Id, ContractId = request.ContractId }, ct);
                await _uow.CommitAsync(ct);

                return Ok(new { id = approvalResult.Id, status = "PendingApproval", message = "添加费用申请已提交审批" });
            }
        }

        // Draft 合同或无审批配置 → 直接写入
        using var conn = _db.CreateConnection(); conn.Open();

        var isOneTime = request.ChargeType == "OneTime";

        var overlap = await conn.QuerySingleAsync<int>(
            _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
            new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                EffectiveDate = request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                ExpiryDate = (string?)null, ExcludeId = (Guid?)null });
        if (overlap > 0)
            return Conflict(new { code = "FEE_CONFIG_OVERLAP", message = "该费用项目在生效日期范围内已存在配置" });

        Guid id;
        if (request.ChargeType == "Recurring")
        {
            // 周期费用：按月拆分
            var ids = await RecurringFeeSplitHelper.InsertMonthlySplitFeeConfigs(
                conn, null, _sql, _billingDomain,
                request.ContractId, request.FeeCodeId,
                request.Amount, request.BillingMode ?? "FixedAmount",
                request.Unit, request.UnitPrice,
                request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                _currentUser.UserId,
                contract.StartDate, contract.EndDate);
            id = ids.Last(); // 用于 ChangeHistory，用长期配置的 ID
        }
        else
        {
            id = Guid.NewGuid();
            await conn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.Default"),
                new { Id = id, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                    BillingMode = request.BillingMode ?? "FixedAmount", Amount = request.Amount,
                    Unit = request.Unit, UnitPrice = request.UnitPrice,
                    EffectiveDate = request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                    CreatedBy = _currentUser.UserId, Now = ChinaTime.Now });
        }
        await InsertChangeHistoryAsync(conn, null, request.ContractId, "FEE_ADD",
            "添加费用", "添加 " + (request.BillingMode ?? "FixedAmount") + " ¥" + request.Amount.ToString("F2") + ", 生效 " + (request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd")),
            null, request.Amount, request.EffectiveDate, _currentUser.UserId);

        // ★ 一次性收费落地后立即生成 JE（草稿合同激活时也会补生成，此处先发可提前记账）
        if (isOneTime)
        {
        }

        return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            var code = ex.Message.Contains("不能早于") ? "EFFECTIVE_DATE_BEFORE_START"
                    : ex.Message.Contains("不能晚于") ? "EFFECTIVE_DATE_AFTER_END"
                    : "VALIDATION_ERROR";
            return BadRequest(new { code, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeeConfigRequest request, CancellationToken ct)
    {
        // 查询费控配置对应的合同，检查是否有待审批
        using var conn = _db.CreateConnection(); conn.Open();
        var config = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Contract.Select.FeeConfig.ContractIdAndAmountById"), new { Id = id });
        if (config == null) return NotFound();
        var contractId = (Guid)config.ContractId;

        var hasPending = await conn.QuerySingleAsync<int>(
            _sql.Get("Approval.Select.Request.PendingByContractId"),
            new { Id = contractId });
        if (hasPending > 0)
            return Conflict(new { code = "PENDING_APPROVAL_EXISTS",
                message = "该合同存在待审批的申请，请处理完成后再修改费用配置" });

        await conn.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.Default"),
            new { Id = id, request.Amount, request.BillingMode, request.Unit, request.UnitPrice, request.IsActive });
        return NoContent();
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustFeeConfigRequest request, CancellationToken ct)
    {
        // Active 合同禁止直接调价，必须走统一的 feeadjust 审批端点
        var contract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct);
        if (contract != null && contract.Status == "Active")
        {
            return BadRequest(new { code = "ACTIVE_CONTRACT_REQUIRES_APPROVAL",
                message = "生效中的合同请使用「费用调价」功能提交审批，不可直接调价" });
        }
        if (contract == null) return NotFound(new { message = "合同不存在" });

        // 校验生效日期在合同起止日期范围内
        Contract.ValidateFeeEffectiveDate(DateTime.Parse(request.EffectiveDate), contract.StartDate, contract.EndDate);

        // Draft 合同直接执行
        using var conn = _db.CreateConnection(); conn.Open();
        var current = await conn.QuerySingleOrDefaultAsync(
            _sql.Get("Lease.Select.ContractFeeConfig.CurrentByContractAndFee"),
            new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId });

        var overlap = await conn.QuerySingleAsync<int>(
            _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
            new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                EffectiveDate = request.EffectiveDate, ExpiryDate = (string?)null,
                ExcludeId = current != null ? (Guid)((dynamic)current).Id : (Guid?)null });
        if (overlap > 0)
            return Conflict(new { code = "FEE_CONFIG_OVERLAP" });

        if (current != null)
        {
            var expiryDate = DateTime.Parse(request.EffectiveDate).AddDays(-1).ToString("yyyy-MM-dd");
            await conn.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.ExpiryDate"),
                new { Id = (Guid)((dynamic)current).Id, ExpiryDate = expiryDate });
            await conn.ExecuteAsync(_sql.Get("Contract.Update.ContractFeeConfig.ExpireByCodeId"),
                new { ExpiryDate = expiryDate, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId });
        }

        var newId = Guid.NewGuid();
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
            new { Id = newId, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                BillingMode = "FixedAmount", Amount = request.NewAmount,
                EffectiveDate = request.EffectiveDate,
                CreatedBy = _currentUser.UserId, Now = ChinaTime.Now });

        await InsertChangeHistoryAsync(conn, null, request.ContractId, "FEE_ADJUST",
            "费用调价", "新金额 " + request.NewAmount.ToString("F2") + "，生效 " + request.EffectiveDate,
            null, request.NewAmount, request.EffectiveDate, _currentUser.UserId);
        return Ok(new { id = newId, message = "调价成功（草稿合同）" });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid contractId, [FromQuery] Guid feeCodeId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync(_sql.Get("Lease.Select.ContractFeeConfig.History"),
            new { ContractId = contractId, FeeCodeId = feeCodeId });
        var list = rows.Select(r => new
        {
            id = (Guid)((dynamic)r).Id,
            amount = (decimal)((dynamic)r).Amount,
            effectiveDate = ((dynamic)r).EffectiveDate is DateTime ed ? ed.ToString("yyyy-MM-dd") : null,
            expiryDate = ((dynamic)r).ExpiryDate is DateTime xd ? xd.ToString("yyyy-MM-dd") : null,
            isActive = (bool)((dynamic)r).IsActive,
            createdAt = ((DateTime)((dynamic)r).CreatedAt).ToString("yyyy-MM-dd HH:mm")
        }).ToList();
        return Ok(list);
    }

    [HttpPost("checkoverlap")]
    public async Task<IActionResult> CheckOverlap([FromBody] CheckOverlapRequest request, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var overlap = await conn.QuerySingleAsync<int>(
            _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
            new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                EffectiveDate = request.EffectiveDate, ExpiryDate = (string?)null,
                ExcludeId = request.ExcludeId });
        return Ok(new { hasOverlap = overlap > 0 });
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

    private static object MapConfig(dynamic r) => new
    {
        id = (Guid)r.Id, contractId = (Guid)r.ContractId, feeCodeId = (Guid)r.FeeCodeId,
        feeCodeName = (string?)r.FeeCodeName, feeCode = (string?)r.FeeCode,
        chargeType = (string?)r.ChargeType ?? "Recurring",
        billingMode = (string)r.BillingMode, amount = (decimal)r.Amount,
        unit = (string?)r.Unit, unitPrice = (decimal?)r.UnitPrice,
        isActive = (bool)r.IsActive,
        effectiveDate = r.EffectiveDate is DateTime ed ? ed.ToString("yyyy-MM-dd") : null,
        expiryDate = r.ExpiryDate is DateTime xd ? xd.ToString("yyyy-MM-dd") : null
    };
}

public class CreateFeeConfigRequest
{
    public Guid ContractId { get; set; }
    public Guid FeeCodeId { get; set; }
    public decimal Amount { get; set; }
    public string? BillingMode { get; set; }
    public string? Unit { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? EffectiveDate { get; set; }
    /// <summary>收费类型：Recurring / OneTime，不传则由 FeeCode 决定</summary>
    public string? ChargeType { get; set; }
}

public class UpdateFeeConfigRequest
{
    public decimal Amount { get; set; }
    public string? BillingMode { get; set; }
    public string? Unit { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdjustFeeConfigRequest
{
    public Guid ContractId { get; set; }
    public Guid FeeCodeId { get; set; }
    public decimal NewAmount { get; set; }
    public string EffectiveDate { get; set; } = "";
}

public class CheckOverlapRequest
{
    public Guid ContractId { get; set; }
    public Guid FeeCodeId { get; set; }
    public string EffectiveDate { get; set; } = "";
    public Guid? ExcludeId { get; set; }
}
