using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

/// <summary>
/// 明细账 — 按科目查询带余额的分录流水
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LedgerController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ITenantService _tenant;

    public LedgerController(IDbConnectionFactory db, ISqlLoader sql, ITenantService tenant)
    {
        _db = db;
        _sql = sql;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetBySubject(
        [FromQuery] string subjectCode,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(subjectCode))
            return BadRequest(new { message = "科目编码不能为空" });

        var companyId = _tenant.EffectiveCompanyId;
        using var conn = _db.CreateConnection();
        conn.Open();

        var entries = await conn.QueryAsync<dynamic>(
            _sql.Get("Accounting.Select.Ledger.BySubject"),
            new { SubjectCode = subjectCode, CompanyId = companyId, StartDate = startDate, EndDate = endDate });

        // 期初余额（截止开始日期前的余额）
        decimal openingBalance = 0;
        if (startDate.HasValue)
        {
            openingBalance = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Accounting.Select.Ledger.OpeningBalance"),
                new { Code = subjectCode, Cid = companyId, Date = startDate.Value });
        }

        var list = entries.Select(e => new
        {
            Id = (Guid)e.Id,
            VoucherId = (Guid)e.VoucherId,
            EntryNo = (int)e.EntryNo,
            VoucherNo = (string)e.VoucherNo,
            VoucherDate = (DateOnly.FromDateTime((DateTime)e.VoucherDate)).ToString("yyyy-MM-dd"),
            Period = (string)e.Period,
            Direction = (string)e.Direction,
            Amount = (decimal)e.Amount,
            Summary = (string?)e.Summary ?? "",
            SubjectCode = (string)e.SubjectCode,
            SubjectName = (string)e.SubjectName,
            Balance = (decimal)e.Balance
        }).ToList();

        return Ok(new
        {
            subjectCode,
            startDate,
            endDate,
            openingBalance,
            entries = list,
            endingBalance = list.Count > 0 ? list.Last().Balance : openingBalance
        });
    }
}
