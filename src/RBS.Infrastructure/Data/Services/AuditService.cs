using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Repositories;
using System.Data;
using System.Data.Common;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计日志查询服务 — 使用原始 SQL 查询 {TableName}_Audit 表
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditEntryDto>> GetHistoryAsync(AuditQuery query, CancellationToken ct = default)
    {
        var tableName = $"{SanitizeTableName(query.TableName)}_Audit";
        var whereClauses = new List<string>();
        var parameters = new List<SqlParameter>();

        // 按记录 ID 筛选
        if (!string.IsNullOrEmpty(query.RecordId))
        {
            whereClauses.Add("[Id] = @recordId");
            parameters.Add(new SqlParameter("@recordId", query.RecordId));
        }

        // 按日期范围筛选
        if (query.StartDate.HasValue)
        {
            whereClauses.Add("[AuditChangedAt] >= @startDate");
            parameters.Add(new SqlParameter("@startDate", query.StartDate.Value));
        }
        if (query.EndDate.HasValue)
        {
            whereClauses.Add("[AuditChangedAt] <= @endDate");
            parameters.Add(new SqlParameter("@endDate", query.EndDate.Value));
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        // 查询总数
        var countSql = $"SELECT COUNT(*) FROM [{tableName}] {whereSql}";
        await using var countCmd = _context.Database.GetDbConnection().CreateCommand();
        countCmd.CommandText = countSql;
        countCmd.Parameters.Clear();
        foreach (var p in parameters)
            countCmd.Parameters.Add(p);

        if (countCmd.Connection!.State != ConnectionState.Open)
            await countCmd.Connection.OpenAsync(ct);

        var countResult = await countCmd.ExecuteScalarAsync(ct);
        var total = countResult != null ? Convert.ToInt32(countResult) : 0;

        // 分页查询
        var offset = (query.Page - 1) * query.PageSize;
        var dataSql = $"SELECT * FROM [{tableName}] {whereSql} ORDER BY [AuditChangedAt] DESC OFFSET {offset} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

        await using var dataCmd = _context.Database.GetDbConnection().CreateCommand();
        dataCmd.CommandText = dataSql;
        dataCmd.Parameters.Clear();
        foreach (var p in parameters)
            dataCmd.Parameters.Add(p);

        if (dataCmd.Connection!.State != ConnectionState.Open)
            await dataCmd.Connection.OpenAsync(ct);

        var items = new List<AuditEntryDto>();
        await using var reader = await dataCmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var changes = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var colName = reader.GetName(i);
                // 跳过审计系统列，这些在 Changes 中不需要重复
                if (colName is "AuditAction" or "AuditVersionNo" or "AuditChangedAt" or "AuditChangedBy")
                    continue;
                changes[colName] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            items.Add(new AuditEntryDto
            {
                Id = $"{reader["Id"]}_{reader["AuditVersionNo"]}",
                EntityId = reader["Id"]?.ToString() ?? "",
                AuditAction = reader["AuditAction"]?.ToString() ?? "",
                AuditVersionNo = reader["AuditVersionNo"] is int v ? v : 1,
                AuditChangedAt = reader["AuditChangedAt"] is DateTime dt ? dt : DateTime.MinValue,
                AuditChangedBy = reader["AuditChangedBy"] is Guid g ? g : Guid.Empty,
                Changes = changes
            });
        }

        return new PagedResult<AuditEntryDto>
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)query.PageSize) : 0
        };
    }

    public async Task<List<AuditCompareDto>> CompareAsync(string tableName, string recordId, int v1, int v2, CancellationToken ct = default)
    {
        var table = $"{SanitizeTableName(tableName)}_Audit";
        var sql = $@"
            SELECT * FROM [{table}]
            WHERE [Id] = @recordId AND [AuditVersionNo] IN (@v1, @v2)
            ORDER BY [AuditVersionNo]";

        await using var cmd = _context.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new SqlParameter("@recordId", recordId));
        cmd.Parameters.Add(new SqlParameter("@v1", v1));
        cmd.Parameters.Add(new SqlParameter("@v2", v2));

        if (cmd.Connection!.State != ConnectionState.Open)
            await cmd.Connection.OpenAsync(ct);

        var rows = new List<DbDataReader>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var versions = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            versions.Add(row);
        }

        if (versions.Count < 2) return new List<AuditCompareDto>();

        var oldVer = versions[0];
        var newVer = versions[1];
        var result = new List<AuditCompareDto>();

        foreach (var key in oldVer.Keys)
        {
            if (key is "AuditAction" or "AuditVersionNo" or "AuditChangedAt" or "AuditChangedBy")
                continue;

            var oldVal = oldVer[key]?.ToString();
            var newVal = newVer.TryGetValue(key, out var nv) ? nv?.ToString() : null;
            var changed = oldVal != newVal;

            result.Add(new AuditCompareDto
            {
                Field = key,
                OldValue = oldVal,
                NewValue = newVal,
                Changed = changed
            });
        }

        return result;
    }

    public async Task<AuditStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var now = RBS.Core.Common.ChinaTime.Now;
        var todayStart = now.Date;
        var weekStart = now.AddDays(-(int)now.DayOfWeek).Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Local);

        // 统计所有审计表（Companies + Menus + Roles + Users）
        await using var cmd = _context.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = @"
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
                4 AS TotalTables";

        cmd.Parameters.Add(new SqlParameter("@todayStart", todayStart));
        cmd.Parameters.Add(new SqlParameter("@weekStart", weekStart));
        cmd.Parameters.Add(new SqlParameter("@monthStart", monthStart));

        if (cmd.Connection!.State != ConnectionState.Open)
            await cmd.Connection.OpenAsync(ct);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new AuditStatsDto
            {
                TodayCount = reader.GetInt32(0),
                WeekCount = reader.GetInt32(1),
                MonthCount = reader.GetInt32(2),
                TotalTables = reader.GetInt32(3)
            };
        }

        return new AuditStatsDto();
    }

    /// <summary>防止 SQL 注入的表名清理</summary>
    private static string SanitizeTableName(string tableName)
    {
        // 只允许字母、数字、下划线
        var sanitized = new string(tableName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return string.IsNullOrEmpty(sanitized) ? "Companies" : sanitized;
    }
}
