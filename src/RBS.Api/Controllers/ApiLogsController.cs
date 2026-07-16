using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Api.Models;
using RBS.Core.Common;
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

    /// <summary>
    /// API 日志列表查询（默认近 7 天，排除 RequestBody/ResponseBody 大字段）
    /// </summary>
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

        // 默认近 7 天日期范围，避免全表扫描（使用东八区时间）
        var chinaNow = ChinaTime.Now;
        startDate ??= chinaNow.AddDays(-7);
        endDate ??= chinaNow.AddDays(1);
        // 前端传东八区时间，转 UTC 后比对（数据库存 UTC）
        where.Add("RequestAt >= @StartDate"); parms.Add("@StartDate", startDate.Value.AddHours(-8));
        where.Add("RequestAt <= @EndDate"); parms.Add("@EndDate", endDate.Value.AddHours(-8));

        var w = "WHERE " + string.Join(" AND ", where);
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", pageSize);

        // 总数 + 数据 一次查询（COUNT(*) OVER() 窗口函数）
        var sql = $@"
            SELECT COUNT(*) OVER() AS Total,
                   Id, HttpMethod, ApiPath, StatusCode, DurationMs, ClientIp, UserId, RequestAt
            FROM ApiLogs {w}
            ORDER BY RequestAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var rows = (await conn.QueryAsync<ApiLogListItem>(sql, parms)).ToList();
        var total = rows.Count > 0 ? rows[0].Total : 0;

        return Ok(new
        {
            items = rows.Select(r => new
            {
                id = r.Id,
                httpMethod = r.HttpMethod,
                path = r.ApiPath,
                statusCode = r.StatusCode,
                durationMs = r.DurationMs,
                ipAddress = r.ClientIp,
                userId = r.UserId,
                userDisplayName = (string?)null,
                createdAt = r.RequestAt.AddHours(8)  // UTC 转东八区
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
        var log = await conn.QuerySingleOrDefaultAsync<ApiLogDetail>(
            @"SELECT Id, HttpMethod, ApiPath, QueryString, RequestBody, StatusCode, ResponseBody,
                     DurationMs, ClientIp, UserAgent, UserId, UserDisplayName, RequestAt
              FROM ApiLogs WHERE Id = @Id", new { Id = id });
        if (log == null) return NotFound();

        return Ok(new
        {
            id = log.Id,
            userId = log.UserId,
            httpMethod = log.HttpMethod,
            path = log.ApiPath,
            queryString = log.QueryString,
            requestBody = log.RequestBody,
            statusCode = log.StatusCode,
            responseBody = log.ResponseBody,
            durationMs = log.DurationMs,
            ipAddress = log.ClientIp,
            userAgent = log.UserAgent,
            userDisplayName = log.UserDisplayName,
            createdAt = log.RequestAt.AddHours(8)  // UTC 转东八区
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
        if (startDate.HasValue) { where.Add("RequestAt >= @StartDate"); parms.Add("@StartDate", startDate.Value.AddHours(-8)); }
        if (endDate.HasValue) { where.Add("RequestAt <= @EndDate"); parms.Add("@EndDate", endDate.Value.AddHours(-8)); }
        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        // 批次删除，每次最多 1000 条，避免锁表
        await conn.ExecuteAsync($@"
            DELETE TOP(1000) FROM ApiLogs {w};
            WHILE @@ROWCOUNT > 0
                DELETE TOP(1000) FROM ApiLogs {w};", parms);
        return NoContent();
    }
}
