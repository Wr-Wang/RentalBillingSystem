namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Scheduling;

/// <summary>
/// 任务步骤日志仓储接口
/// </summary>
public interface ITaskStepLogRepository
{
    Task<Guid> CreateAsync(TaskStepLog log, CancellationToken ct = default);
    Task CompleteAsync(Guid id, int affectedCount, CancellationToken ct = default);
    Task FailAsync(Guid id, string error, CancellationToken ct = default);
    Task<List<TaskStepLog>> GetByTaskLogIdAsync(Guid taskLogId, CancellationToken ct = default);
}
