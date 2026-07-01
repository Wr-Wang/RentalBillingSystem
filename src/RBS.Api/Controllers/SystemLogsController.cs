using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;
using System.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemLogsController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    public SystemLogsController(IDbConnectionFactory db) => _db = db;

    private bool IsSuperAdmin => User.FindFirst("IsSuperAdmin")?.Value == "True";
    private IActionResult? RequireSuperAdmin() => IsSuperAdmin ? null : Forbid();

    private static readonly string Columns = @"
        Id AS id, Level AS level, Message AS message, Exception AS exception, Source AS source, Path AS path, Method AS method,
        IpAddress AS ipAddress, UserAgent AS userAgent,
        UserId AS userId, UserDisplayName AS userDisplayName,
        CreatedAt AS createdAt";

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? level = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();

        var where = new List<string>();
        var parms = new DynamicParameters();
        if (!string.IsNullOrEmpty(level)) { where.Add("Level = @Level"); parms.Add("@Level", level); }
        if (startDate.HasValue) { where.Add("CreatedAt >= @StartDate"); parms.Add("@StartDate", startDate.Value); }
        if (endDate.HasValue) { where.Add("CreatedAt <= @EndDate"); parms.Add("@EndDate", endDate.Value); }

        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", pageSize);

        var total = await conn.QuerySingleAsync<int>($"SELECT COUNT(*) FROM SystemLogs {w}", parms);
        var items = await conn.QueryAsync($"SELECT {Columns} FROM SystemLogs {w} ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", parms);

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();
        var log = await conn.QuerySingleOrDefaultAsync($"SELECT {Columns} FROM SystemLogs WHERE Id = @Id", new { Id = id });
        if (log == null) return NotFound();
        return Ok(log);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("DELETE FROM SystemLogs WHERE Id = @Id", new { Id = id });
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearAll(CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("DELETE FROM SystemLogs");
        return NoContent();
    }
}
