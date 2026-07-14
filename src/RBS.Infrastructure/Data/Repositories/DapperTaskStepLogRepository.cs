using Dapper;
using RBS.Core.Entities.Scheduling;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

/// <summary>
/// Dapper 任务步骤日志仓储实现 — 管理调度任务中每个步骤的执行日志
/// </summary>
/// <remarks>
/// 功能：创建步骤日志、完成/失败更新、按任务日志 ID 查询所有步骤。
/// 每个任务日志可包含多个步骤日志，用于跟踪调度任务的分步骤执行过程。
/// 所有 SQL 从 SqlMaps.xml 加载。
/// </remarks>
public class DapperTaskStepLogRepository : ITaskStepLogRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    /// <summary>
    /// 初始化步骤日志仓储
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="sql">SQL 映射加载器</param>
    public DapperTaskStepLogRepository(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    /// <summary>
    /// 创建步骤日志并返回生成的 ID
    /// </summary>
    /// <param name="log">步骤日志实体</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新建步骤日志的 ID</returns>
    public async Task<Guid> CreateAsync(TaskStepLog log, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Insert.TaskStepLog.Default"), log);
        return log.Id;
    }

    /// <summary>
    /// 完成任务步骤日志 — 更新影响行数和完成状态
    /// </summary>
    /// <param name="id">步骤日志 ID</param>
    /// <param name="affectedCount">步骤影响的数据行数</param>
    /// <param name="ct">取消令牌</param>
    public async Task CompleteAsync(Guid id, int affectedCount, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskStepLog.Complete"),
            new { Id = id, Count = affectedCount });
    }

    /// <summary>
    /// 标记步骤日志为失败状态
    /// </summary>
    /// <param name="id">步骤日志 ID</param>
    /// <param name="error">错误信息</param>
    /// <param name="ct">取消令牌</param>
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
