using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractFeeConfigsController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ICurrentUserService _currentUser;

    public ContractFeeConfigsController(IDbConnectionFactory db, ISqlLoader sql, ICurrentUserService currentUser)
    {
        _db = db;
        _sql = sql;
        _currentUser = currentUser;
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
        using var conn = _db.CreateConnection(); conn.Open();

        // 区间不交叉校验
        var overlap = await conn.QuerySingleAsync<int>(
            _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
            new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                EffectiveDate = request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                ExpiryDate = (string?)null, ExcludeId = (Guid?)null });
        if (overlap > 0)
            return Conflict(new { error = "该费用项目在生效日期范围内已存在配置，请调整生效日期" });

        var id = Guid.NewGuid();
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.Default"),
            new
            {
                Id = id, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                BillingMode = request.BillingMode ?? "FixedAmount",
                Amount = request.Amount, Unit = request.Unit, UnitPrice = request.UnitPrice,
                EffectiveDate = request.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                CreatedBy = _currentUser.UserId, Now = ChinaTime.Now
            });
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeeConfigRequest request, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.Default"),
            new { Id = id, request.Amount, request.BillingMode, request.Unit, request.UnitPrice, request.IsActive });
        return NoContent();
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustFeeConfigRequest request, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();

        // 查找当前生效记录
        var current = await conn.QuerySingleOrDefaultAsync(
            _sql.Get("Lease.Select.ContractFeeConfig.CurrentByContractAndFee"),
            new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId });

        // 生效日校验：新生效日必须大于原生效日，否则原配置到期日 < 生效日
        if (current != null)
        {
            var curEff = (DateTime)((dynamic)current).EffectiveDate;
            var newEff = DateOnly.Parse(request.EffectiveDate);
            if (newEff <= DateOnly.FromDateTime(curEff))
                return BadRequest(new { error = $"生效日期必须晚于当前配置的生效日期 {curEff:yyyy-MM-dd}" });
        }

        // 区间不交叉校验（排除当前生效记录自身）
        var overlap = await conn.QuerySingleAsync<int>(
            _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
            new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                EffectiveDate = request.EffectiveDate, ExpiryDate = (string?)null,
                ExcludeId = current != null ? (Guid)((dynamic)current).Id : (Guid?)null });
        if (overlap > 0)
            return Conflict(new { error = "该费用项目在新生效日期范围内已存在配置，请调整生效日期" });

        // 存在则设为到期 + 停用
        if (current != null)
        {
            var expiryDate = DateTime.Parse(request.EffectiveDate).AddDays(-1).ToString("yyyy-MM-dd");
            await conn.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.ExpiryDate"),
                new { Id = (Guid)((dynamic)current).Id, ExpiryDate = expiryDate });
            await conn.ExecuteAsync(
                _sql.Get("Contract.Update.ContractFeeConfig.ExpireByCodeId"),
                new { ExpiryDate = expiryDate, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId });
        }

        // 创建新记录
        var newId = Guid.NewGuid();
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
            new
            {
                Id = newId, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                BillingMode = "FixedAmount", Amount = request.NewAmount,
                EffectiveDate = request.EffectiveDate,
                CreatedBy = _currentUser.UserId, Now = ChinaTime.Now
            });

        return Ok(new { id = newId, message = "调价成功" });
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

    private static object MapConfig(dynamic r) => new
    {
        id = (Guid)r.Id,
        contractId = (Guid)r.ContractId,
        feeCodeId = (Guid)r.FeeCodeId,
        feeCodeName = (string?)r.FeeCodeName,
        feeCode = (string?)r.FeeCode,
        billingMode = (string)r.BillingMode,
        amount = (decimal)r.Amount,
        unit = (string?)r.Unit,
        unitPrice = (decimal?)r.UnitPrice,
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
