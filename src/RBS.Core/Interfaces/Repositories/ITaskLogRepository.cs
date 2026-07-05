namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Scheduling;

/// <summary>
/// 任务执行日志仓储接口
/// </summary>
public interface ITaskLogRepository
{
    Task<Guid> CreateAsync(TaskLog taskLog, CancellationToken ct = default);
    Task<TaskLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TaskLog>> GetByTaskNameAsync(string taskName, Guid companyId, string targetMonth, CancellationToken ct = default);
    Task CompleteAsync(Guid id, int totalCount, int successCount, int failCount, int warningCount, string? summary, CancellationToken ct = default);
    Task FailAsync(Guid id, string error, CancellationToken ct = default);
    Task UpdateHeartbeatAsync(Guid id, CancellationToken ct = default);
    Task SetDryRunResultAsync(Guid id, string resultData, CancellationToken ct = default);
    Task<List<TaskLog>> GetStaleTasksAsync(TimeSpan timeout, CancellationToken ct = default);
    Task MarkStaleAsync(Guid id, CancellationToken ct = default);
}
