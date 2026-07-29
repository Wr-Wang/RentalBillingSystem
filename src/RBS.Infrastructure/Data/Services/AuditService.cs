using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Infrastructure.Data.Configs;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计日志查询服务 — 使用 Dapper 查询 {TableName}_Audit 表
/// </summary>
/// <remarks>
/// v2 增强：
/// <list type="bullet">
///   <item><description>通过 AuditFieldConfigLoader 获取实体中文名和关键字段配置</description></item>
///   <item><description>展示时分离"关键信息区"和"变更数据区"</description></item>
///   <item><description>读取 AuditChangedFields 列，精准定位变更字段</description></item>
/// </list>
/// </remarks>
public class AuditService : IAuditService
{
    private readonly IDbConnectionFactory _db;
    private readonly AuditFieldConfigLoader _configLoader;

    /// <summary>
    /// 初始化审计服务
    /// </summary>
    public AuditService(IDbConnectionFactory db, AuditFieldConfigLoader configLoader)
    {
        _db = db;
        _configLoader = configLoader;
    }

    /// <summary>
    /// 分页查询审计日志历史
    /// </summary>
    public async Task<PagedResult<AuditEntryDto>> GetHistoryAsync(AuditQuery query, CancellationToken ct = default)
    {
        // 配置查找：先精确匹配表名，再尝试单数化匹配
        var config = _configLoader.GetConfig(query.TableName);
        if (config == null)
        {
            // 尝试去掉末尾的 s 匹配单数形式（如 Companies → companies）
            var singular = query.TableName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                ? query.TableName[..^1] : query.TableName;
            config = _configLoader.GetConfig(singular);
        }

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

            var auditAction = dict.ContainsKey("AuditAction") ? dict["AuditAction"]?.ToString() : "Unknown";
            var changedFieldsStr = dict.ContainsKey("AuditChangedFields") ? dict["AuditChangedFields"]?.ToString() : null;

            // ---- 提取关键字段 ----
            var keyValues = new Dictionary<string, object?>();
            if (config != null)
            {
                foreach (var kf in config.KeyFields)
                {
                    if (dict.ContainsKey(kf))
                        keyValues[kf] = dict[kf];
                }
            }
            // 如果没有任何关键字段匹配，至少展示 Id
            if (keyValues.Count == 0 && dict.ContainsKey("Id"))
            {
                keyValues["Id"] = dict["Id"];
            }

            // ---- 解析变更字段 ----
            var changedFieldNames = new List<string>();
            var changedValues = new Dictionary<string, object?>();

            if (auditAction == "Update" && !string.IsNullOrEmpty(changedFieldsStr))
            {
                changedFieldNames = changedFieldsStr
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
                foreach (var fn in changedFieldNames)
                {
                    if (dict.ContainsKey(fn))
                        changedValues[fn] = dict[fn];
                }
            }

            return new AuditEntryDto
            {
                Id = dict.ContainsKey("AuditId")
                    ? $"{dict["Id"]}_{dict["AuditVersionNo"]}"
                    : $"{dict["Id"]}_{dict["AuditVersionNo"]}",
                EntityId = dict["Id"]?.ToString() ?? "",
                AuditAction = auditAction ?? "",
                AuditVersionNo = dict["AuditVersionNo"] is int v ? v : 1,
                AuditChangedAt = dict["AuditChangedAt"] is DateTime dt ? dt : DateTime.MinValue,
                AuditChangedBy = dict["AuditChangedBy"] is Guid g ? g : Guid.Empty,
                EntityDisplayName = !string.IsNullOrEmpty(config?.DisplayName) ? config.DisplayName : query.TableName,
                KeyValues = keyValues,
                ChangedFieldNames = changedFieldNames,
                ChangedValues = changedValues,
            };
        }).ToList();

        return new PagedResult<AuditEntryDto>
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)query.PageSize) : 0
        };
    }

    /// <summary>
    /// 对比指定记录的两个审计版本之间的字段差异
    /// </summary>
    public async Task<List<AuditCompareDto>> CompareAsync(
        string tableName, string recordId, int v1, int v2, CancellationToken ct = default)
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
            if (key is "AuditId" or "AuditAction" or "AuditVersionNo" or "AuditChangedAt"
                or "AuditChangedBy" or "AuditChangedHostname" or "AuditChangedFields")
                continue;
            result.Add(new AuditCompareDto
            {
                Field = key,
                OldValue = oldDict[key]?.ToString(),
                NewValue = newDict.TryGetValue(key, out var nv) ? nv?.ToString() : null,
                Changed = oldDict[key]?.ToString() !=
                          (newDict.TryGetValue(key, out var nv2) ? nv2?.ToString() : null)
            });
        }
        return result;
    }

    /// <summary>
    /// 审计统计
    /// </summary>
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

    /// <summary>
    /// 获取所有已配置的审计表清单（供前端动态加载下拉列表）
    /// </summary>
    public List<AuditTableInfo> GetAuditTables()
    {
        return _configLoader.GetAllTables();
    }

    /// <summary>
    /// 回滚到指定版本 — 从 _Audit 表读取版本数据，恢复主表
    /// </summary>
    public async Task<AuditRollbackResult> RollbackAsync(string tableName, string recordId, int versionNo, CancellationToken ct = default)
    {
        var auditTable = $"{SanitizeTableName(tableName)}_Audit";

        using var conn = _db.CreateConnection();
        conn.Open();

        // 1. 检查审计表是否存在
        var tableExists = await conn.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM sys.tables WHERE name = @Name", new { Name = auditTable });
        if (tableExists == 0)
        {
            return new AuditRollbackResult
            {
                Success = false,
                ErrorCode = "TABLE_NOT_FOUND",
                ErrorMessage = $"审计表 {auditTable} 不存在"
            };
        }

        // 2. 读取指定版本的审计记录
        var auditRow = await conn.QuerySingleOrDefaultAsync<dynamic>(
            $"SELECT * FROM [{auditTable}] WHERE Id=@Id AND AuditVersionNo=@Ver",
            new { Id = recordId, Ver = versionNo });

        if (auditRow == null)
        {
            return new AuditRollbackResult
            {
                Success = false,
                ErrorCode = "VERSION_NOT_FOUND",
                ErrorMessage = $"未找到版本 {versionNo} 的审计记录"
            };
        }

        // 3. 获取最新版本（对比差异）
        var latestVersion = await conn.QuerySingleOrDefaultAsync<dynamic>(
            $"SELECT TOP 1 * FROM [{auditTable}] WHERE Id=@Id ORDER BY AuditVersionNo DESC",
            new { Id = recordId });

        var sourceRow = latestVersion ?? auditRow;

        // 4. 构建 UPDATE 语句恢复主表
        var updateFields = new List<string>();
        var updateParms = new Dictionary<string, object?>();
        var rowDict = ((IDictionary<string, object?>)sourceRow);

        foreach (var kv in rowDict)
        {
            var key = kv.Key;
            if (key is "AuditId" or "AuditAction" or "AuditVersionNo" or "AuditChangedAt" or "AuditChangedBy"
                or "AuditChangedHostname" or "AuditChangedFields" or "RowVersion" or "Id")
                continue;
            updateFields.Add($"[{key}]=@{key}");
            updateParms[key] = kv.Value;
        }

        updateParms["Id"] = recordId;
        var sql = $"UPDATE [{tableName}] SET {string.Join(", ", updateFields)} WHERE Id=@Id";
        await conn.ExecuteAsync(sql, updateParms);

        return new AuditRollbackResult
        {
            Success = true,
            Table = tableName,
            RecordId = recordId,
            VersionNo = versionNo,
            ErrorMessage = $"已回滚到版本 {versionNo}"
        };
    }

    private static string SanitizeTableName(string tableName)
    {
        var sanitized = new string(tableName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return string.IsNullOrEmpty(sanitized) ? "Companies" : sanitized;
    }
}
