namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 审计日志写入服务 — 将实体变更写入 {TableName}_Audit 表
/// 使用独立数据库连接，与主事务完全解耦。
/// </summary>
public interface IAuditLogWriter
{
    /// <summary>记录实体变更审计</summary>
    Task LogChangesAsync(string tableName, string entityId, string action, Dictionary<string, object?> changes, Guid changedBy, CancellationToken ct = default);
}
