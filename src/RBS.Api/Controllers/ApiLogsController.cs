using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiLogsController : ControllerBase
{
    private readonly IDbConnectionFactory _db;

    public ApiLogsController(IDbConnectionFactory db) => _db = db;

    private bool IsSuperAdmin => User.FindFirst("IsSuperAdmin")?.Value == "True";
    private IActionResult? RequireSuperAdmin() => IsSuperAdmin ? null : Forbid();

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? method = null, [FromQuery] string? path = null,
        [FromQuery] int? statusCode = null, [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();

        var where = new List<string>();
        var parms = new DynamicParameters();

        if (!string.IsNullOrEmpty(method)) { where.Add("HttpMethod = @Method"); parms.Add("@Method", method); }
        if (!string.IsNullOrEmpty(path)) { where.Add("Path LIKE @Path"); parms.Add("@Path", $"%{path}%"); }
        if (statusCode.HasValue) { where.Add("StatusCode = @StatusCode"); parms.Add("@StatusCode", statusCode.Value); }
        if (userId.HasValue) { where.Add("UserId = @UserId"); parms.Add("@UserId", userId.Value); }
        if (startDate.HasValue) { where.Add("CreatedAt >= @StartDate"); parms.Add("@StartDate", startDate.Value); }
        if (endDate.HasValue) { where.Add("CreatedAt <= @EndDate"); parms.Add("@EndDate", endDate.Value); }

        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", pageSize);

        var total = await conn.QuerySingleAsync<int>($"SELECT COUNT(*) FROM ApiLogs {w}", parms);
        var items = await conn.QueryAsync($"SELECT * FROM ApiLogs {w} ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", parms);

        return Ok(new
        {
            items = items.Select(l => new
            {
                ((dynamic)l).Id, ((dynamic)l).HttpMethod, ((dynamic)l).Path,
                ((dynamic)l).QueryString, ((dynamic)l).StatusCode,
                ((dynamic)l).DurationMs, ((dynamic)l).IpAddress,
                ((dynamic)l).UserId, ((dynamic)l).UserDisplayName, ((dynamic)l).CreatedAt
            }),
            total, page, pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();
        var log = await conn.QuerySingleOrDefaultAsync("SELECT * FROM ApiLogs WHERE Id = @Id", new { Id = id });
        if (log == null) return NotFound();
        return Ok(log);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("DELETE FROM ApiLogs WHERE Id = @Id", new { Id = id });
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteByRange(
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();

        var where = new List<string>();
        var parms = new DynamicParameters();
        if (startDate.HasValue) { where.Add("CreatedAt >= @StartDate"); parms.Add("@StartDate", startDate.Value); }
        if (endDate.HasValue) { where.Add("CreatedAt <= @EndDate"); parms.Add("@EndDate", endDate.Value); }
        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        await conn.ExecuteAsync($"DELETE FROM ApiLogs {w}", parms);
        return NoContent();
    }
}
