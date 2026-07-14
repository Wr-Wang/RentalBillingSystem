namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Scheduling;

/// <summary>
/// 任务步骤日志仓储接口。
/// 定义任务步骤日志的特有操作方法，提供创建、完成、失败
/// 以及按任务日志 ID 查询等业务方法。
/// 步骤日志用于记录定时任务中各步骤的执行细节，支持更精细的任务监控。
/// </summary>
public interface ITaskStepLogRepository
{
    /// <summary>
    /// 创建任务步骤日志记录。
    /// </summary>
    /// <param name="log">步骤日志实体</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的步骤日志 ID</returns>
    Task<Guid> CreateAsync(TaskStepLog log, CancellationToken ct = default);

    /// <summary>
    /// 标记步骤执行成功，并记录影响记录数。
    /// </summary>
    /// <param name="id">步骤日志 ID</param>
    /// <param name="affectedCount">该步骤影响的记录数</param>
    /// <param name="ct">取消令牌</param>
    Task CompleteAsync(Guid id, int affectedCount, CancellationToken ct = default);

    /// <summary>
    /// 标记步骤执行失败，并记录错误信息。
    /// </summary>
    /// <param name="id">步骤日志 ID</param>
    /// <param name="error">错误信息</param>
    /// <param name="ct">取消令牌</param>
    Task FailAsync(Guid id, string error, CancellationToken ct = default);

    /// <summary>
    /// 根据任务日志 ID 获取该任务下的所有步骤日志。
    /// </summary>
    /// <param name="taskLogId">任务日志 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>步骤日志列表</returns>
    Task<List<TaskStepLog>> GetByTaskLogIdAsync(Guid taskLogId, CancellationToken ct = default);
}
