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
    private readonly ISqlLoader _sql;
    public ApiLogsController(IDbConnectionFactory db, ISqlLoader sql) { _db = db; _sql = sql; }

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
        if (!string.IsNullOrEmpty(path)) { where.Add("ApiPath LIKE @Path"); parms.Add("@Path", $"%{path}%"); }
        if (statusCode.HasValue) { where.Add("StatusCode = @StatusCode"); parms.Add("@StatusCode", statusCode.Value); }
        if (userId.HasValue) { where.Add("UserId = @UserId"); parms.Add("@UserId", userId.Value); }
        if (startDate.HasValue) { where.Add("RequestAt >= @StartDate"); parms.Add("@StartDate", startDate.Value); }
        if (endDate.HasValue) { where.Add("RequestAt <= @EndDate"); parms.Add("@EndDate", endDate.Value); }

        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", pageSize);

        var total = Convert.ToInt32(await conn.ExecuteScalarAsync($"SELECT COUNT(*) FROM ApiLogs {w}", parms));
        var items = await conn.QueryAsync($"SELECT * FROM ApiLogs {w} ORDER BY RequestAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", parms);

        return Ok(new
        {
            items = items.Select(l =>
            {
                dynamic r = l;
                return new
                {
                    id = (Guid)r.Id,
                    httpMethod = (string)r.HttpMethod,
                    path = (string)r.ApiPath,
                    statusCode = (int)r.StatusCode,
                    durationMs = (int)r.DurationMs,
                    ipAddress = (string?)r.ClientIp,
                    userId = (Guid?)r.UserId,
                    createdAt = (DateTime)r.RequestAt,
                    userDisplayName = (string?)null   // 暂为 null，待数据库加列后补全
                };
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

        dynamic d = log;
        return Ok(new
        {
            id = (Guid)d.Id,
            userId = (Guid?)d.UserId,
            httpMethod = (string)d.HttpMethod,
            path = (string)d.ApiPath,
            queryString = (string?)null,     // 暂为 null，待数据库加列后补全
            requestBody = (string?)d.RequestBody,
            statusCode = (int)d.StatusCode,
            responseBody = (string?)d.ResponseBody,
            durationMs = (long)(int)d.DurationMs,
            ipAddress = (string?)d.ClientIp,
            userAgent = (string?)null,       // 暂为 null，待数据库加列后补全
            userDisplayName = (string?)null, // 暂为 null，待数据库加列后补全
            createdAt = (DateTime)d.RequestAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Common.Delete.ApiLog.ById"), new { Id = id });
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
        if (startDate.HasValue) { where.Add("RequestAt >= @StartDate"); parms.Add("@StartDate", startDate.Value); }
        if (endDate.HasValue) { where.Add("RequestAt <= @EndDate"); parms.Add("@EndDate", endDate.Value); }
        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        await conn.ExecuteAsync($"DELETE FROM ApiLogs {w}", parms);
        return NoContent();
    }
}
