using System.Data;
using Dapper;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;

namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 步骤级日志记录器实现
/// 步骤日志与业务数据在同一事务中写入，提交=记录，回滚=不留痕
/// </summary>
public class TaskStepLogger : ITaskStepLogger
{
    private readonly ITaskStepLogRepository _stepRepo;
    private readonly IDbConnectionFactory _db;

    public TaskStepLogger(ITaskStepLogRepository stepRepo, IDbConnectionFactory db)
    {
        _stepRepo = stepRepo;
        _db = db;
    }

    public async Task<Guid> StartStepAsync(Guid taskLogId, string stepName, string displayName,
        Guid? parentId = null, IDbTransaction? tx = null, CancellationToken ct = default)
    {
        var log = new RBS.Core.Entities.Scheduling.TaskStepLog(taskLogId, stepName, displayName, parentId);
        if (tx?.Connection != null)
        {
            await tx.Connection.ExecuteAsync(
                @"INSERT INTO TaskStepLogs (Id, TaskLogId, StepName, StepDisplayName,
                  ParentId, SortOrder, Status, StartedAt)
                  VALUES (@Id, @TaskLogId, @StepName, @StepDisplayName,
                  @ParentId, @SortOrder, @Status, @StartedAt)", log, tx);
        }
        else
        {
            await _stepRepo.CreateAsync(log, ct);
        }
        return log.Id;
    }

    public async Task CompleteStepAsync(Guid stepLogId, int affectedCount,
        IDbTransaction? tx = null, CancellationToken ct = default)
    {
        var sql = @"UPDATE TaskStepLogs SET Status = 'Completed', CompletedAt = GETUTCDATE(),
            DurationMs = DATEDIFF(MILLISECOND, StartedAt, GETUTCDATE()),
            AffectedCount = @Count WHERE Id = @Id";

        if (tx?.Connection != null)
            await tx.Connection.ExecuteAsync(sql, new { Id = stepLogId, Count = affectedCount }, tx);
        else
            await _stepRepo.CompleteAsync(stepLogId, affectedCount, ct);
    }

    public async Task FailStepAsync(Guid stepLogId, string error,
        IDbTransaction? tx = null, CancellationToken ct = default)
    {
        var sql = @"UPDATE TaskStepLogs SET Status = 'Failed', CompletedAt = GETUTCDATE(),
            DurationMs = DATEDIFF(MILLISECOND, StartedAt, GETUTCDATE()),
            ErrorMessage = @Error WHERE Id = @Id";

        if (tx?.Connection != null)
            await tx.Connection.ExecuteAsync(sql, new { Id = stepLogId, Error = error }, tx);
        else
            await _stepRepo.FailAsync(stepLogId, error, ct);
    }

    public async Task SkipStepAsync(Guid stepLogId, string reason,
        IDbTransaction? tx = null, CancellationToken ct = default)
    {
        var sql = "UPDATE TaskStepLogs SET Status = 'Skipped', Message = @Reason WHERE Id = @Id";

        if (tx?.Connection != null)
            await tx.Connection.ExecuteAsync(sql, new { Id = stepLogId, Reason = reason }, tx);
        else
        {
            using var conn = _db.CreateConnection(); conn.Open();
            await conn.ExecuteAsync(sql, new { Id = stepLogId, Reason = reason });
        }
    }
}
