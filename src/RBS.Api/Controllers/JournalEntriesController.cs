using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ITenantService _tenant;

    public JournalEntriesController(IDbConnectionFactory db, ISqlLoader sql, ITenantService tenant)
    {
        _db = db;
        _sql = sql;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var companyId = _tenant.EffectiveCompanyId;
        var offset = (page - 1) * pageSize;

        using var conn = _db.CreateConnection();
        conn.Open();

        var items = await conn.QueryAsync(
            _sql.Get("Accounting.Select.JournalEntry.Paged"),
            new { CompanyId = companyId, StartDate = startDate, EndDate = endDate, Offset = offset, PageSize = pageSize });

        var total = await conn.QuerySingleAsync<int>(
            _sql.Get("Accounting.Select.JournalEntry.PagedCount"),
            new { CompanyId = companyId, StartDate = startDate, EndDate = endDate });

        return Ok(new { items, total, page, pageSize });
    }
}
