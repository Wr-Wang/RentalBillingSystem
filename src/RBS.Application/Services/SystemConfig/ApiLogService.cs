using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.Services.SystemConfig;

/// <summary>
/// API 日志应用服务实现 — 查询与清理 API 调用日志
/// </summary>
public class ApiLogService : IApiLogService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ApiLogService(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    public async Task<(List<ApiLogListItemDto> Items, int Total)> GetListAsync(
        int page, int pageSize, string? method, string? path,
        int? statusCode, Guid? userId, DateTime? startDate, DateTime? endDate,
        CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

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
        where.Add("RequestAt >= @StartDate"); parms.Add("@StartDate", startDate.Value.AddHours(-8));
        where.Add("RequestAt <= @EndDate"); parms.Add("@EndDate", endDate.Value.AddHours(-8));

        var w = "WHERE " + string.Join(" AND ", where);
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", pageSize);

        var sql = $@"
            SELECT COUNT(*) OVER() AS Total,
                   Id, HttpMethod, ApiPath, StatusCode, DurationMs, ClientIp, UserId, RequestAt
            FROM ApiLogs {w}
            ORDER BY RequestAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var rows = (await conn.QueryAsync<ApiLogListItemDto>(sql, parms)).ToList();
        var total = rows.Count > 0 ? rows[0].Total : 0;

        return (rows, total);
    }

    public async Task<ApiLogDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        var log = await conn.QuerySingleOrDefaultAsync<ApiLogDetailDto>(
            @"SELECT Id, HttpMethod, ApiPath, QueryString, RequestBody, StatusCode, ResponseBody,
                     DurationMs, ClientIp, UserAgent, UserId, UserDisplayName, RequestAt
              FROM ApiLogs WHERE Id = @Id", new { Id = id });

        return log;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        await conn.ExecuteAsync(_sql.Get("Common.Delete.ApiLog.ById"), new { Id = id });
    }

    public async Task DeleteByRangeAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        if (!startDate.HasValue && !endDate.HasValue)
        {
            await conn.ExecuteAsync("TRUNCATE TABLE ApiLogs");
        }
        else
        {
            var where = new List<string>();
            var parms = new DynamicParameters();
            if (startDate.HasValue) { where.Add("RequestAt >= @StartDate"); parms.Add("@StartDate", startDate.Value.AddHours(-8)); }
            if (endDate.HasValue) { where.Add("RequestAt <= @EndDate"); parms.Add("@EndDate", endDate.Value.AddHours(-8)); }
            var w = "WHERE " + string.Join(" AND ", where);

            await conn.ExecuteAsync($@"
                DELETE TOP(1000) FROM ApiLogs {w};
                WHILE @@ROWCOUNT > 0
                    DELETE TOP(1000) FROM ApiLogs {w};", parms);
        }
    }
}
