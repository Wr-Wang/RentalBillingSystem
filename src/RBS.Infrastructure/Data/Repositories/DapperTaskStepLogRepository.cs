using Dapper;
using RBS.Core.Entities.Scheduling;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class DapperTaskStepLogRepository : ITaskStepLogRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public DapperTaskStepLogRepository(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    public async Task<Guid> CreateAsync(TaskStepLog log, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Insert.TaskStepLog.Default"), log);
        return log.Id;
    }

    public async Task CompleteAsync(Guid id, int affectedCount, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskStepLog.Complete"),
            new { Id = id, Count = affectedCount });
    }

    public async Task FailAsync(Guid id, string error, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskStepLog.Fail"),
            new { Id = id, Error = error });
    }

    public async Task<List<TaskStepLog>> GetByTaskLogIdAsync(Guid taskLogId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<TaskStepLog>(
            _sql.Get("Scheduling.Select.TaskStepLog.ByTaskLogId"),
            new { Id = taskLogId });
        return rows.ToList();
    }
}
