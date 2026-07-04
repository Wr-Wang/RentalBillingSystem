using Microsoft.Data.SqlClient;
using RBS.Core.Common;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计日志写入实现 — 使用独立 SqlConnection 写入 {TableName}_Audit 表
/// 与主业务事务完全解耦，失败不影响主操作。
/// </summary>
public class AuditLogWriter : IAuditLogWriter
{
    private readonly string _connectionString;

    public AuditLogWriter(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 记录审计日志 — 独立连接、独立事务、失败静默
    /// </summary>
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
