using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计日志查询服务 — 使用 Dapper 查询 {TableName}_Audit 表
/// </summary>
/// <remarks>
/// 提供三种查询能力：
/// <list type="bullet">
///   <item><description>GetHistoryAsync — 分页查询审计历史（支持按记录 ID/日期范围筛选）</description></item>
///   <item><description>CompareAsync — 对比两个版本的字段差异</description></item>
///   <item><description>GetStatsAsync — 统计今日/本周/本月审计记录量和表数量</description></item>
/// </list>
/// 表名使用 SanitizeTableName 消毒，防止 SQL 注入。
/// </remarks>
public class AuditService : IAuditService
{
    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// 初始化审计服务
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    public AuditService(IDbConnectionFactory db) => _db = db;

    /// <summary>
    /// 分页查询审计日志历史
    /// </summary>
    /// <remarks>
    /// SQL 策略：
    /// <list type="bullet">
    ///   <item><description>动态拼接 WHERE 条件（记录 ID、开始日期、结束日期均为可选）</description></item>
    ///   <item><description>先 COUNT 查总数，再 OFFSET-FETCH 分页</description></item>
    ///   <item><description>审计元字段（AuditAction/AuditVersionNo/AuditChangedAt/AuditChangedBy）从 Changes 字典中排除</description></item>
    /// </list>
    /// </remarks>
    /// <param name="query">审计查询参数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>分页审计条目</returns>
    public async Task<PagedResult<AuditEntryDto>> GetHistoryAsync(AuditQuery query, CancellationToken ct = default)
    {
        var tableName = $"{SanitizeTableName(query.TableName)}_Audit";
        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(query.RecordId))
        {
            whereClauses.Add("[Id] = @recordId");
            parameters.Add("@recordId", query.RecordId);
        }
        if (query.StartDate.HasValue)
        {
            whereClauses.Add("[AuditChangedAt] >= @startDate");
            parameters.Add("@startDate", query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            whereClauses.Add("[AuditChangedAt] <= @endDate");
            parameters.Add("@endDate", query.EndDate.Value);
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        using var conn = _db.CreateConnection();
        conn.Open();

        var total = Convert.ToInt32(await conn.ExecuteScalarAsync(
            $"SELECT COUNT(*) FROM [{tableName}] {whereSql}", parameters));

        var offset = (query.Page - 1) * query.PageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", query.PageSize);

        var rows = await conn.QueryAsync(
            $"SELECT * FROM [{tableName}] {whereSql} ORDER BY [AuditChangedAt] DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            parameters);

        var items = rows.Select(r =>
        {
            var dict = (IDictionary<string, object>)r;
            var changes = new Dictionary<string, object?>();
            foreach (var kv in dict)
            {
                if (kv.Key is "AuditAction" or "AuditVersionNo" or "AuditChangedAt" or "AuditChangedBy")
                    continue;
                changes[kv.Key] = kv.Value;
            }
            return new AuditEntryDto
            {
                Id = $"{dict["Id"]}_{dict["AuditVersionNo"]}",
                EntityId = dict["Id"]?.ToString() ?? "",
                AuditAction = dict["AuditAction"]?.ToString() ?? "",
                AuditVersionNo = dict["AuditVersionNo"] is int v ? v : 1,
                AuditChangedAt = dict["AuditChangedAt"] is DateTime dt ? dt : DateTime.MinValue,
                AuditChangedBy = dict["AuditChangedBy"] is Guid g ? g : Guid.Empty,
                Changes = changes
            };
        }).ToList();

        return new PagedResult<AuditEntryDto>
        {
            Items = items, Total = total, Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)query.PageSize) : 0
        };
    }

    /// <summary>
    /// 对比指定记录的两个审计版本之间的字段差异
    /// </summary>
    /// <param name="tableName">业务表名</param>
    /// <param name="recordId">记录 ID</param>
    /// <param name="v1">旧版本号</param>
    /// <param name="v2">新版本号</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>字段级别差异列表（每个字段含新旧值和是否变更标记）</returns>
    public async Task<List<AuditCompareDto>> CompareAsync(string tableName, string recordId, int v1, int v2, CancellationToken ct = default)
    {
        var table = $"{SanitizeTableName(tableName)}_Audit";
        using var conn = _db.CreateConnection(); conn.Open();

        var rows = (await conn.QueryAsync(
            $"SELECT * FROM [{table}] WHERE [Id] = @recordId AND [AuditVersionNo] IN (@v1, @v2) ORDER BY [AuditVersionNo]",
            new { recordId, v1, v2 })).ToList();

        if (rows.Count < 2) return new List<AuditCompareDto>();

        var oldDict = (IDictionary<string, object>)rows[0];
        var newDict = (IDictionary<string, object>)rows[1];
        var result = new List<AuditCompareDto>();

        foreach (var key in oldDict.Keys)
        {
            if (key is "AuditAction" or "AuditVersionNo" or "AuditChangedAt" or "AuditChangedBy")
                continue;
            result.Add(new AuditCompareDto
            {
                Field = key,
                OldValue = oldDict[key]?.ToString(),
                NewValue = newDict.TryGetValue(key, out var nv) ? nv?.ToString() : null,
                Changed = oldDict[key]?.ToString() != (newDict.TryGetValue(key, out var nv2) ? nv2?.ToString() : null)
            });
        }
        return result;
    }

    public async Task<AuditStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var now = RBS.Core.Common.ChinaTime.Now;
        using var conn = _db.CreateConnection(); conn.Open();

        var tables = (await conn.QueryAsync<string>(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%_Audit%'")).ToList();

        long todaySum = 0, weekSum = 0, monthSum = 0;
        var weekStart = now.AddDays(-(int)now.DayOfWeek).Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        foreach (var t in tables)
        {
            var today = Convert.ToInt64(await conn.ExecuteScalarAsync(
                $"SELECT COUNT(*) FROM [{t}] WHERE [AuditChangedAt] >= @d0",
                new { d0 = now.Date }));
            todaySum += today;
            var week = Convert.ToInt64(await conn.ExecuteScalarAsync(
                $"SELECT COUNT(*) FROM [{t}] WHERE [AuditChangedAt] >= @d0",
                new { d0 = weekStart }));
            weekSum += week;
            var month = Convert.ToInt64(await conn.ExecuteScalarAsync(
                $"SELECT COUNT(*) FROM [{t}] WHERE [AuditChangedAt] >= @d0",
                new { d0 = monthStart }));
            monthSum += month;
        }

        return new AuditStatsDto
        {
            TodayCount = (int)todaySum,
            WeekCount = (int)weekSum,
            MonthCount = (int)monthSum,
            TotalTables = tables.Count
        };
    }

    private static string SanitizeTableName(string tableName)
    {
        var sanitized = new string(tableName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return string.IsNullOrEmpty(sanitized) ? "Companies" : sanitized;
    }
}
