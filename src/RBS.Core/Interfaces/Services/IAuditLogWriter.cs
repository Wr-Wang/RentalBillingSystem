namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 审计日志写入服务 — 将实体变更写入 {TableName}_Audit 表。
/// 与 IAuditService 的区别在于显式接收 changedBy 参数，
/// 适用于不需要从 ICurrentUserService 解析操作人信息的场景（如后台任务）。
/// 使用独立数据库连接，与主事务完全解耦。
/// </summary>
public interface IAuditLogWriter
{
    /// <summary>
    /// 记录实体变更审计。
    /// 将实体的变更操作信息异步写入审计表中，
    /// 通过显式传入操作人 ID 以支持后台任务等非用户触发场景的审计。
    /// </summary>
    /// <param name="tableName">实体表名（如 Contracts、ReceivablePlans）</param>
    /// <param name="entityId">实体主键值的字符串表示</param>
    /// <param name="action">操作类型（Insert/Update/Delete）</param>
    /// <param name="changes">变更的字段名→值字典</param>
    /// <param name="changedBy">操作人用户 ID</param>
    /// <param name="ct">取消令牌</param>
    Task LogChangesAsync(string tableName, string entityId, string action, Dictionary<string, object?> changes, Guid changedBy, CancellationToken ct = default);
}
