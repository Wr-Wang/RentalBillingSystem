using Microsoft.Data.SqlClient;
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
///   <item><description>与主业务事务完全解耦，审计写入失败不影响主操作（静默吞异常）</description></item>
///   <item><description>首次 INSERT 前检查审计表是否存在（IF EXISTS sys.tables）</description></item>
///   <item><description>使用 SqlCommand 参数化查询，防止 SQL 注入</description></item>
///   <item><description>动态拼接列名（由 changes 字典驱动），Id/RowVersion 字段自动排除</description></item>
/// </list>
/// 设计模式：独立连接写入 + 失败静默（Fire-and-Forget with fail-silent）。
/// </remarks>
public class AuditLogWriter : IAuditLogWriter
{
    private readonly string _connectionString;

    /// <summary>
    /// 初始化审计日志写入器
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    public AuditLogWriter(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 记录审计日志 — 独立连接、独立事务、失败静默
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
        try
        {
            var auditTableName = $"{tableName}_Audit";

            var columns = new List<string>
            {
                "Id", "AuditAction", "AuditVersionNo", "AuditChangedAt", "AuditChangedBy"
            };
            var values = new List<object?>
            {
                entityId, action, 1, ChinaTime.Now, changedBy
            };

            foreach (var kv in changes)
            {
                if (kv.Key is "Id" or "RowVersion") continue;
                columns.Add($"[{kv.Key}]");
                values.Add(kv.Value);
            }

            var paramNames = values.Select((_, i) => $"@p{i}");

            var sql = $@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = @tableName)
    INSERT INTO [{auditTableName}] ({string.Join(", ", columns)})
    VALUES ({string.Join(", ", paramNames)})";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tableName", auditTableName);
            for (int i = 0; i < values.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", values[i] ?? DBNull.Value);
            }

            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            // 审计写入失败绝不影响主表操作 — 静默吞异常
        }
    }
}
