using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Services.SystemConfig;

/// <summary>
/// 系统日志查询与管理服务
/// </summary>
public class SystemLogService : ISystemLogService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public SystemLogService(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    private static readonly string Columns = @"
        Id AS id, Level AS level, Message AS message, Exception AS exception, Source AS source, Path AS path, Method AS method,
        IpAddress AS ipAddress, UserAgent AS userAgent,
        UserId AS userId, UserDisplayName AS userDisplayName,
        CreatedAt AS createdAt";

    public async Task<PagedResult<SystemLogDto>> GetListAsync(
        int page, int pageSize, string? level,
        DateTime? startDate, DateTime? endDate,
        CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        var where = new List<string>();
        var parms = new DynamicParameters();
        if (!string.IsNullOrEmpty(level)) { where.Add("Level = @Level"); parms.Add("@Level", level); }
        if (startDate.HasValue) { where.Add("CreatedAt >= @StartDate"); parms.Add("@StartDate", startDate.Value); }
        if (endDate.HasValue) { where.Add("CreatedAt <= @EndDate"); parms.Add("@EndDate", endDate.Value); }

        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", pageSize);

        var total = await conn.QuerySingleAsync<int>(string.Format(_sql.Get("Common.Select.SystemLog.Count"), w), parms);
        var items = (await conn.QueryAsync<SystemLogDto>(string.Format(_sql.Get("Common.Select.SystemLog.Paged"), Columns, w), parms)).ToList();

        return new PagedResult<SystemLogDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SystemLogDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        return await conn.QuerySingleOrDefaultAsync<SystemLogDto>(
            string.Format(_sql.Get("Common.Select.SystemLog.ById"), Columns),
            new { Id = id });
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        await conn.ExecuteAsync(_sql.Get("Common.Delete.SystemLog.ById"), new { Id = id });
    }

    public async Task ClearAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        await conn.ExecuteAsync(_sql.Get("Common.Delete.SystemLog.All"));
    }
}
