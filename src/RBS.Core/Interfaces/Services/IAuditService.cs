namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 审计服务接口 — 将实体变更写入 {TableName}_Audit 表。
/// 使用独立数据库连接，与主事务完全解耦，确保审计日志不会因主事务回滚而丢失。
/// 每条审计记录包含表名、实体 ID、操作类型（Insert/Update/Delete）以及变更字段明细。
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// 记录实体变更审计。
    /// 将实体的变更操作信息异步写入审计表中，
    /// 使用独立连接提交，不受工作单元事务影响。
    /// </summary>
    /// <param name="tableName">实体表名（如 Users、ApprovalRequests）</param>
    /// <param name="entityId">实体主键值的字符串表示</param>
    /// <param name="action">操作类型（Insert/Update/Delete）</param>
    /// <param name="changes">变更的字段名→值字典，已自动过滤无需审计的字段</param>
    /// <param name="ct">取消令牌</param>
    Task LogChangesAsync(string tableName, string entityId, string action, Dictionary<string, object?> changes, CancellationToken ct = default);
}
