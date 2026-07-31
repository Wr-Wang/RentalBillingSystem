using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using RBS.Core.Common;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计日志写入实现 — 使用独立 SqlConnection 写入 {TableName}_Audit 表
/// </summary>
/// <remarks>
/// 架构设计：
/// <list type="bullet">
///   <item><description>使用独立的连接字符串（与业务库相同但连接独立）</description></item>
///   <item><description>与主业务事务完全解耦，审计写入失败不影响主操作（仅 LogWarning）</description></item>
///   <item><description>写入前检查审计表是否存在（IF EXISTS sys.tables）</description></item>
///   <item><description>AuditVersionNo 自动递增（MAX + 1），支持同一记录多次修改</description></item>
///   <item><description>使用 SqlCommand 参数化查询，防止 SQL 注入</description></item>
///   <item><description>动态拼接列名（由 changes 字典驱动），Id 字段自动排除</description></item>
///   <item><description>v2 增强：写入 AuditChangedFields 列标记变更字段，支持前端精准展示</description></item>
/// </list>
/// 设计模式：独立连接写入 + 失败 LogWarning（Fire-and-Forget with fail-logged）。
/// </remarks>
public class AuditLogWriter : IAuditLogWriter
{
    private readonly string _connectionString;
    private readonly ILogger<AuditLogWriter> _logger;

    /// <summary>
    /// 初始化审计日志写入器
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="logger">日志记录器（审计失败仅记录 Warning，不影响主操作）</param>
    public AuditLogWriter(string connectionString, ILogger<AuditLogWriter> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// 记录审计日志 — 独立连接、独立事务、失败仅 LogWarning
    /// </summary>
    /// <param name="tableName">业务表名（将自动附加 _Audit 后缀）</param>
    /// <param name="entityId">实体 ID</param>
    /// <param name="action">操作类型（Create/Update/Delete）</param>
    /// <param name="changes">变化的字段字典</param>
    /// <param name="changedBy">操作人 ID</param>
    /// <param name="ct">取消令牌</param>
    public async Task LogChangesAsync(
        string tableName, string entityId, string action,
        Dictionary<string, object?> changes, Guid changedBy, CancellationToken ct = default)
    {
        var auditTableName = $"{tableName}_Audit";

        try
        {
            // ---- 计算 AuditChangedFields ----
            string? changedFields = ComputeChangedFields(auditTableName, entityId, action, changes, ct);

            // ---- 构建列名和参数 ----
            var columns = new List<string>
            {
                "[Id]", "[AuditAction]", "[AuditVersionNo]", "[AuditChangedAt]", "[AuditChangedBy]"
            };
            var values = new List<object?>
            {
                entityId, action, ChinaTime.Now, changedBy
            };
            var paramRefs = new List<string> { "@p0", "@p1", "@nextVer", "@p2", "@p3" };

            // 提取主机名用于 AuditChangedHostname 元数据列（不影响实体数据列的写入）
            string? hostname = null;
            foreach (var kv in changes)
            {
                if (kv.Key is "Id") continue;
                if (hostname == null &&
                    (string.Equals(kv.Key, "CreatedHostname", StringComparison.Ordinal) ||
                     string.Equals(kv.Key, "UpdatedHostname", StringComparison.Ordinal)))
                {
                    hostname = kv.Value?.ToString();
                }
                columns.Add($"[{kv.Key}]");
                values.Add(kv.Value);
                paramRefs.Add($"@p{values.Count - 1}");
            }
            if (hostname != null)
            {
                columns.Add("[AuditChangedHostname]");
                paramRefs.Add("@hostname");
            }

            // ---- 添加 AuditChangedFields 列 ----
            if (changedFields != null)
            {
                columns.Add("[AuditChangedFields]");
                paramRefs.Add("@changedFields");
            }

            // ---- 执行插入 ----
            var sql = $@"
DECLARE @nextVer INT = ISNULL((SELECT MAX(AuditVersionNo) FROM [{auditTableName}] WHERE [Id] = @p0), 0) + 1;
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = @tableName)
    INSERT INTO [{auditTableName}] ({string.Join(", ", columns)})
    VALUES ({string.Join(", ", paramRefs)});";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tableName", auditTableName);
            for (int i = 0; i < values.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", values[i] ?? DBNull.Value);
            }
            if (hostname != null)
            {
                cmd.Parameters.AddWithValue("@hostname", hostname);
            }
            if (changedFields != null)
            {
                cmd.Parameters.AddWithValue("@changedFields", changedFields);
            }

            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "审计写入失败（不影响主操作）: Table={AuditTable}, EntityId={EntityId}, Action={Action}",
                auditTableName, entityId, action);
        }
    }

    /// <summary>
    /// 计算 AuditChangedFields 值
    /// </summary>
    private string? ComputeChangedFields(string auditTableName, string entityId, string action,
        Dictionary<string, object?> changes, CancellationToken ct)
    {
        if (action == "Delete")
            return "DELETED";

        if (action != "Update")
            return null; // Insert → null 表示全部新建

        // Update：读取上一版本，计算差异
        try
        {
            var previous = ReadPreviousVersionAsync(auditTableName, entityId, ct)
                .GetAwaiter().GetResult();
            if (previous == null)
                return null;

            var changedList = new List<string>();
            foreach (var kv in changes)
            {
                // 跳过审计元数据列
                if (kv.Key is "Id" or "AuditAction" or "AuditVersionNo" or "AuditChangedAt"
                    or "AuditChangedBy" or "AuditChangedHostname" or "AuditChangedFields")
                    continue;

                if (!previous.TryGetValue(kv.Key, out var oldVal) || !AreEqual(oldVal, kv.Value))
                {
                    changedList.Add(kv.Key);
                }
            }
            return changedList.Count > 0 ? string.Join(",", changedList) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "读取审计上一版本失败（不影响审计写入）: Table={Table}, Id={Id}",
                auditTableName, entityId);
            return null;
        }
    }

    /// <summary>
    /// 读取审计表中最新的版本记录
    /// </summary>
    private async Task<Dictionary<string, object?>?> ReadPreviousVersionAsync(
        string auditTableName, string entityId, CancellationToken ct)
    {
        var sql = $"SELECT TOP 1 * FROM [{auditTableName}] WHERE [Id] = @Id ORDER BY [AuditVersionNo] DESC";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", entityId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var dict = new Dictionary<string, object?>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var val = reader.GetValue(i);
            dict[reader.GetName(i)] = val == DBNull.Value ? null : val;
        }
        return dict;
    }

    /// <summary>
    /// 比较两个值是否相等（处理 DBNull、null、byte[]、DateTime 等特殊类型）
    /// </summary>
    private static bool AreEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        // DBNull 视为 null
        if (a is DBNull) a = null;
        if (b is DBNull) b = null;
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        // byte[] 比较（如二进制字段）
        if (a is byte[] ba && b is byte[] bb)
            return ba.AsSpan().SequenceEqual(bb);

        // 直接 Equals
        return a.Equals(b);
    }
}
