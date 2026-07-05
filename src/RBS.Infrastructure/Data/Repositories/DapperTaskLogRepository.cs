using Dapper;
using RBS.Core.Entities.Scheduling;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class DapperTaskLogRepository : ITaskLogRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public DapperTaskLogRepository(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    public async Task<Guid> CreateAsync(TaskLog taskLog, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Insert.TaskLog.Default"), taskLog);
        return taskLog.Id;
    }

    public async Task<TaskLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<TaskLog>(
            _sql.Get("Scheduling.Select.TaskLog.ById"), new { Id = id });
    }

    public async Task<List<TaskLog>> GetByTaskNameAsync(string taskName, Guid companyId, string targetMonth, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<TaskLog>(
            _sql.Get("Scheduling.Select.TaskLog.ByName"),
            new { Name = taskName, Cid = companyId, Month = targetMonth });
        return rows.ToList();
    }

    public async Task CompleteAsync(Guid id, int totalCount, int successCount, int failCount, int warningCount, string? summary, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.Complete"),
            new { Id = id, Total = totalCount, Success = successCount, Fail = failCount, Warn = warningCount, Summary = summary });
    }

    public async Task FailAsync(Guid id, string error, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.Fail"),
            new { Id = id, Error = error });
    }

    public async Task UpdateHeartbeatAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.Heartbeat"),
            new { Id = id });
    }

    public async Task SetDryRunResultAsync(Guid id, string resultData, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.DryRunResult"),
            new { Id = id, Data = resultData });
    }

    public async Task<List<TaskLog>> GetStaleTasksAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<TaskLog>(
            _sql.Get("Scheduling.Select.TaskLog.Stale"),
            new { Min = (int)timeout.TotalMinutes });
        return rows.ToList();
    }

    public async Task MarkStaleAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.MarkStale"),
            new { Id = id });
    }
}
