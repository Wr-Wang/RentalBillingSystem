using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractFeeConfigsController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    public ContractFeeConfigsController(IDbConnectionFactory db) => _db = db;

    /// <summary>获取合同的所有费用版本（含历史），按生效日期排序</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        if (contractId == null) return Ok(new List<object>());
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync(@"
            SELECT cf.Id, cf.ContractId, cf.FeeCodeId, fc.Name AS FeeCodeName, fc.Code AS FeeCode,
                   cf.BillingMode, cf.Amount, cf.Unit, cf.UnitPrice, cf.IsActive,
                   cf.EffectiveDate, cf.ExpiryDate
            FROM ContractFeeConfigs cf
            LEFT JOIN FeeCodes fc ON fc.Id = cf.FeeCodeId
            WHERE cf.ContractId = @ContractId
            ORDER BY cf.FeeCodeId, cf.EffectiveDate DESC", new { ContractId = contractId.Value });
        var list = rows.Select(r => new
        {
            id = (Guid)((dynamic)r).Id,
            contractId = (Guid)((dynamic)r).ContractId,
            feeCodeId = (Guid)((dynamic)r).FeeCodeId,
            feeCodeName = (string?)((dynamic)r).FeeCodeName,
            feeCode = (string?)((dynamic)r).FeeCode,
            billingMode = (string)((dynamic)r).BillingMode,
            amount = (decimal)((dynamic)r).Amount,
            unit = (string?)((dynamic)r).Unit,
            unitPrice = (decimal?)((dynamic)r).UnitPrice,
            isActive = (bool)((dynamic)r).IsActive,
            effectiveDate = ((dynamic)r).EffectiveDate is DateTime ed ? ed.ToString("yyyy-MM-dd") : null,
            expiryDate = ((dynamic)r).ExpiryDate is DateTime xd ? xd.ToString("yyyy-MM-dd") : null
        }).ToList();
        return Ok(list);
    }

    /// <summary>新增费用（首次）</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] System.Text.Json.JsonElement body, CancellationToken ct)
    {
        var contractId = body.GetProperty("contractId").GetGuid();
        var feeCodeId = body.GetProperty("feeCodeId").GetGuid();
        var amount = body.GetProperty("amount").GetDecimal();
        var billingMode = body.TryGetProperty("billingMode", out var bm) ? bm.GetString() ?? "FixedAmount" : "FixedAmount";
        var unit = body.TryGetProperty("unit", out var u) ? u.GetString() : null;
        var unitPrice = body.TryGetProperty("unitPrice", out var up) && up.ValueKind == System.Text.Json.JsonValueKind.Number ? up.GetDecimal() : (decimal?)null;
        var effectiveDate = body.TryGetProperty("effectiveDate", out var ed) ? ed.GetString() : null;
        if (string.IsNullOrEmpty(effectiveDate)) effectiveDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        using var conn = _db.CreateConnection(); conn.Open();
        var id = Guid.NewGuid();
        var userId = GetUserId();
        await conn.ExecuteAsync(@"
            INSERT INTO ContractFeeConfigs (Id, ContractId, FeeCodeId, BillingMode, Amount, Unit, UnitPrice, IsActive, EffectiveDate, ExpiryDate, CreatedBy, CreatedAt)
            VALUES (@Id, @ContractId, @FeeCodeId, @BillingMode, @Amount, @Unit, @UnitPrice, 1, @EffectiveDate, NULL, @CreatedBy, @Now)",
            new { Id = id, ContractId = contractId, FeeCodeId = feeCodeId, BillingMode = billingMode,
                  Amount = amount, Unit = unit, UnitPrice = unitPrice, EffectiveDate = effectiveDate,
                  CreatedBy = userId, Now = DateTime.UtcNow });
        return Ok(new { id });
    }

    /// <summary>更新费用元数据（金额/计费方式/启用停用）</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] System.Text.Json.JsonElement body, CancellationToken ct)
    {
        var amount = body.GetProperty("amount").GetDecimal();
        var billingMode = body.TryGetProperty("billingMode", out var bm) ? bm.GetString() ?? "FixedAmount" : "FixedAmount";
        var unit = body.TryGetProperty("unit", out var u) ? u.GetString() : null;
        var unitPrice = body.TryGetProperty("unitPrice", out var up) && up.ValueKind == System.Text.Json.JsonValueKind.Number ? up.GetDecimal() : (decimal?)null;
        var isActive = body.TryGetProperty("isActive", out var ia) ? ia.GetBoolean() : true;

        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(@"
            UPDATE ContractFeeConfigs SET Amount=@Amount, BillingMode=@BillingMode, Unit=@Unit, UnitPrice=@UnitPrice, IsActive=@IsActive
            WHERE Id=@Id",
            new { Id = id, Amount = amount, BillingMode = billingMode, Unit = unit, UnitPrice = unitPrice, IsActive = isActive });
        return NoContent();
    }

    /// <summary>调价专用接口：旧记录到期 + 创建新记录</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] System.Text.Json.JsonElement body, CancellationToken ct)
    {
        var contractId = body.GetProperty("contractId").GetGuid();
        var feeCodeId = body.GetProperty("feeCodeId").GetGuid();
        var newAmount = body.GetProperty("newAmount").GetDecimal();
        var effectiveDate = body.GetProperty("effectiveDate").GetString()!;

        using var conn = _db.CreateConnection(); conn.Open();
        var userId = GetUserId();

        // 1. 查找当前生效的记录（ExpiryDate IS NULL 且 IsActive=1）
        var current = await conn.QuerySingleOrDefaultAsync(@"
            SELECT Id FROM ContractFeeConfigs
            WHERE ContractId=@ContractId AND FeeCodeId=@FeeCodeId
              AND ExpiryDate IS NULL AND IsActive=1",
            new { ContractId = contractId, FeeCodeId = feeCodeId });

        // 2. 如果存在当前生效记录，设其到期日为生效日前一天
        if (current != null)
        {
            var expiryDate = DateTime.Parse(effectiveDate).AddDays(-1).ToString("yyyy-MM-dd");
            await conn.ExecuteAsync("UPDATE ContractFeeConfigs SET ExpiryDate=@ExpiryDate WHERE Id=@Id",
                new { Id = (Guid)((dynamic)current).Id, ExpiryDate = expiryDate });
        }

        // 3. 创建新记录
        var newId = Guid.NewGuid();
        var billingMode = "FixedAmount";
        await conn.ExecuteAsync(@"
            INSERT INTO ContractFeeConfigs (Id, ContractId, FeeCodeId, BillingMode, Amount, Unit, UnitPrice, IsActive, EffectiveDate, ExpiryDate, CreatedBy, CreatedAt)
            VALUES (@Id, @ContractId, @FeeCodeId, @BillingMode, @Amount, NULL, NULL, 1, @EffectiveDate, NULL, @CreatedBy, @Now)",
            new
            {
                Id = newId, ContractId = contractId, FeeCodeId = feeCodeId,
                BillingMode = billingMode, Amount = newAmount,
                EffectiveDate = effectiveDate, CreatedBy = userId, Now = DateTime.UtcNow
            });

        return Ok(new { id = newId, message = "调价成功" });
    }

    /// <summary>获取指定费用项的版本历史</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid contractId, [FromQuery] Guid feeCodeId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync(@"
            SELECT Id, Amount, EffectiveDate, ExpiryDate, IsActive, CreatedAt
            FROM ContractFeeConfigs
            WHERE ContractId=@ContractId AND FeeCodeId=@FeeCodeId
            ORDER BY EffectiveDate DESC",
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

    private Guid? GetUserId()
    {
        var idStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return idStr != null && Guid.TryParse(idStr, out var id) ? id : null;
    }
}
