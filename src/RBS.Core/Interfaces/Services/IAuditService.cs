namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 审计服务接口 — 将实体变更写入 {TableName}_Audit 表
/// 使用独立数据库连接，与主事务完全解耦。
/// </summary>
public interface IAuditService
{
    /// <summary>记录实体变更审计</summary>
    /// <param name="tableName">实体表名（如 Users、ApprovalRequests）</param>
    /// <param name="entityId">实体主键值</param>
    /// <param name="action">操作类型（Insert/Update/Delete）</param>
    /// <param name="changes">变更的字段名→值字典（不含 RowVersion）</param>
    Task LogChangesAsync(string tableName, string entityId, string action, Dictionary<string, object?> changes, CancellationToken ct = default);
}
