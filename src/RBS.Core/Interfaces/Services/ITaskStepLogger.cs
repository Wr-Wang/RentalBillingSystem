namespace RBS.Core.Interfaces.Services;

using System.Data;

/// <summary>
/// 步骤级日志记录器。
/// 步骤日志与业务数据在同一事务中写入，确保任务执行状态与业务数据一致。
/// 提供步骤的开始、完成、失败和跳过四个阶段的日志记录能力，
/// 支持指定数据库事务参数以保持与业务操作的事务一致性。
/// </summary>
public interface ITaskStepLogger
{
    /// <summary>
    /// 开始记录一个步骤。
    /// 在步骤执行前调用，创建步骤日志记录并返回步骤日志 ID。
    /// 支持父子步骤层级结构（如：主步骤 → 子步骤1、子步骤2）。
    /// </summary>
    /// <param name="taskLogId">所属任务日志 ID</param>
    /// <param name="stepName">步骤内部名称（英文标识）</param>
    /// <param name="displayName">步骤显示名称（中文描述）</param>
    /// <param name="parentId">父步骤日志 ID（支持层级结构，可选）</param>
    /// <param name="tx">数据库事务，用于与业务操作保持事务一致性（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的步骤日志 ID</returns>
    Task<Guid> StartStepAsync(Guid taskLogId, string stepName, string displayName,
        Guid? parentId = null, IDbTransaction? tx = null, CancellationToken ct = default);

    /// <summary>
    /// 标记步骤执行成功，并记录该步骤影响的记录数。
    /// </summary>
    /// <param name="stepLogId">步骤日志 ID</param>
    /// <param name="affectedCount">该步骤影响的业务记录数</param>
    /// <param name="tx">数据库事务（可选）</param>
    /// <param name="ct">取消令牌</param>
    Task CompleteStepAsync(Guid stepLogId, int affectedCount,
        IDbTransaction? tx = null, CancellationToken ct = default);

    /// <summary>
    /// 标记步骤执行失败，并记录错误信息。
    /// </summary>
    /// <param name="stepLogId">步骤日志 ID</param>
    /// <param name="error">错误信息描述</param>
    /// <param name="tx">数据库事务（可选）</param>
    /// <param name="ct">取消令牌</param>
    Task FailStepAsync(Guid stepLogId, string error,
        IDbTransaction? tx = null, CancellationToken ct = default);

    /// <summary>
    /// 跳过步骤，记录跳过原因。
    /// 用于条件性执行的步骤，当条件不满足时跳过而非标记失败。
    /// </summary>
    /// <param name="stepLogId">步骤日志 ID</param>
    /// <param name="reason">跳过原因说明</param>
    /// <param name="tx">数据库事务（可选）</param>
    /// <param name="ct">取消令牌</param>
    Task SkipStepAsync(Guid stepLogId, string reason,
        IDbTransaction? tx = null, CancellationToken ct = default);
}
