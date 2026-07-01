using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计日志查询服务 — 使用 Dapper 查询 {TableName}_Audit 表
/// </summary>
public class AuditService : IAuditService
{
    private readonly IDbConnectionFactory _db;

    public AuditService(IDbConnectionFactory db) => _db = db;

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

        var total = await conn.QuerySingleAsync<int>(
            $"SELECT COUNT(*) FROM [{tableName}] {whereSql}", parameters);

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

        var result = await conn.QuerySingleAsync(@"
            SELECT
                ISNULL((SELECT COUNT(*) FROM [Companies_Audit] WHERE [AuditChangedAt] >= @todayStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Menus_Audit] WHERE [AuditChangedAt] >= @todayStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Roles_Audit] WHERE [AuditChangedAt] >= @todayStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Users_Audit] WHERE [AuditChangedAt] >= @todayStart), 0) AS TodayCount,
                ISNULL((SELECT COUNT(*) FROM [Companies_Audit] WHERE [AuditChangedAt] >= @weekStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Menus_Audit] WHERE [AuditChangedAt] >= @weekStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Roles_Audit] WHERE [AuditChangedAt] >= @weekStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Users_Audit] WHERE [AuditChangedAt] >= @weekStart), 0) AS WeekCount,
                ISNULL((SELECT COUNT(*) FROM [Companies_Audit] WHERE [AuditChangedAt] >= @monthStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Menus_Audit] WHERE [AuditChangedAt] >= @monthStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Roles_Audit] WHERE [AuditChangedAt] >= @monthStart), 0)
                + ISNULL((SELECT COUNT(*) FROM [Users_Audit] WHERE [AuditChangedAt] >= @monthStart), 0) AS MonthCount,
                4 AS TotalTables",
            new { todayStart = now.Date, weekStart = now.AddDays(-(int)now.DayOfWeek).Date,
                  monthStart = new DateTime(now.Year, now.Month, 1) });

        return new AuditStatsDto
        {
            TodayCount = (int)result.TodayCount,
            WeekCount = (int)result.WeekCount,
            MonthCount = (int)result.MonthCount,
            TotalTables = (int)result.TotalTables
        };
    }

    private static string SanitizeTableName(string tableName)
    {
        var sanitized = new string(tableName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return string.IsNullOrEmpty(sanitized) ? "Companies" : sanitized;
    }
}
