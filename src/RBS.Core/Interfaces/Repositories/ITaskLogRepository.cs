namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Scheduling;

/// <summary>
/// 任务执行日志仓储接口。
/// 定义任务执行日志的特有操作方法，提供创建、查询、完成、失败、
/// 心跳更新、模拟运行结果保存、超时任务检测和标记等业务方法。
/// 注意：ITaskLogRepository 不继承 IRepository，因为 TaskLog 不是 AuditableEntity，
/// 而是采用独立的原始 SQL 操作实现。
/// </summary>
public interface ITaskLogRepository
{
    /// <summary>
    /// 创建任务执行日志记录。
    /// </summary>
    /// <param name="taskLog">任务日志实体</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的任务日志 ID</returns>
    Task<Guid> CreateAsync(TaskLog taskLog, CancellationToken ct = default);

    /// <summary>
    /// 根据 ID 获取任务执行日志。
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>任务日志实体，不存在时返回 null</returns>
    Task<TaskLog?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 根据任务名称、公司和目标月份获取任务执行日志列表。
    /// 用于查看同一任务的历史执行记录。
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <param name="companyId">公司 ID</param>
    /// <param name="targetMonth">目标月份，格式"yyyy-MM"</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>任务日志列表</returns>
    Task<List<TaskLog>> GetByTaskNameAsync(string taskName, Guid companyId, string targetMonth, CancellationToken ct = default);

    /// <summary>
    /// 标记任务执行完成，并记录执行统计信息。
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="totalCount">总处理记录数</param>
    /// <param name="successCount">成功处理记录数</param>
    /// <param name="failCount">失败处理记录数</param>
    /// <param name="warningCount">警告记录数</param>
    /// <param name="summary">执行摘要说明</param>
    /// <param name="ct">取消令牌</param>
    Task CompleteAsync(Guid id, int totalCount, int successCount, int failCount, int warningCount, string? summary, CancellationToken ct = default);

    /// <summary>
    /// 标记任务执行失败，并记录错误信息。
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="error">错误信息</param>
    /// <param name="ct">取消令牌</param>
    Task FailAsync(Guid id, string error, CancellationToken ct = default);

    /// <summary>
    /// 更新任务心跳时间，用于长时间运行的任务保持活跃状态。
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="ct">取消令牌</param>
    Task UpdateHeartbeatAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 保存模拟运行（DryRun）的结果数据。
    /// 用于预览模式查看任务执行预期结果而不实际提交变更。
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="resultData">模拟运行结果 JSON 数据</param>
    /// <param name="ct">取消令牌</param>
    Task SetDryRunResultAsync(Guid id, string resultData, CancellationToken ct = default);

    /// <summary>
    /// 获取超过指定超时时间的陈旧任务日志。
    /// 用于检测和处理处于运行状态但实际已超时未响应的任务。
    /// </summary>
    /// <param name="timeout">超时时间间隔</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>陈旧任务日志列表</returns>
    Task<List<TaskLog>> GetStaleTasksAsync(TimeSpan timeout, CancellationToken ct = default);

    /// <summary>
    /// 将任务标记为陈旧（超时未响应）。
    /// 通常用于对长时间未更新心跳的任务进行超时处理。
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="ct">取消令牌</param>
    Task MarkStaleAsync(Guid id, CancellationToken ct = default);
}
