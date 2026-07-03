using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var id = Guid.NewGuid();
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.Default"),
            new
            {
                Id = id, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                BillingMode = request.BillingMode ?? "FixedAmount",
                Amount = request.Amount, Unit = request.Unit, UnitPrice = request.UnitPrice,
                EffectiveDate = request.EffectiveDate ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                CreatedBy = _currentUser.UserId, Now = DateTime.UtcNow
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

        // 存在则设到期
        if (current != null)
        {
            var expiryDate = DateTime.Parse(request.EffectiveDate).AddDays(-1).ToString("yyyy-MM-dd");
            await conn.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.ExpiryDate"),
                new { Id = (Guid)((dynamic)current).Id, ExpiryDate = expiryDate });
        }

        // 创建新记录
        var newId = Guid.NewGuid();
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
            new
            {
                Id = newId, ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
                BillingMode = "FixedAmount", Amount = request.NewAmount,
                EffectiveDate = request.EffectiveDate,
                CreatedBy = _currentUser.UserId, Now = DateTime.UtcNow
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
            effectiveDate = ((DateTime)((dynamic)r).EffectiveDate).ToString("yyyy-MM-dd"),
            expiryDate = ((dynamic)r).ExpiryDate is DateTime ed ? ed.ToString("yyyy-MM-dd") : null,
            isActive = (bool)((dynamic)r).IsActive,
            createdAt = ((DateTime)((dynamic)r).CreatedAt).ToString("yyyy-MM-dd HH:mm")
        }).ToList();
        return Ok(list);
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
