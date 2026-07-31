using Dapper;
using RBS.Core.Common;
using RBS.Core.Entities.Scheduling;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

/// <summary>
/// Dapper 任务日志仓储实现 — 管理调度任务的执行日志
/// </summary>
/// <remarks>
/// 功能：创建任务日志、按名称查询、完成/失败/心跳/模拟运行结果更新、僵死任务检测与标记。
/// 所有 SQL 均从 SqlMaps.xml 加载。
/// </remarks>
public class DapperTaskLogRepository : ITaskLogRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    /// <summary>
    /// 初始化任务日志仓储
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="sql">SQL 映射加载器</param>
    public DapperTaskLogRepository(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    /// <summary>
    /// 创建任务日志并返回生成的 ID
    /// </summary>
    /// <param name="taskLog">任务日志实体</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新建任务的 ID</returns>
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

    /// <summary>
    /// 根据任务名称、公司和目标月份查询任务日志
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <param name="companyId">公司 ID</param>
    /// <param name="targetMonth">目标月份（格式 yyyy-MM）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>匹配的任务日志列表</returns>
    public async Task<List<TaskLog>> GetByTaskNameAsync(string taskName, Guid companyId, string targetMonth, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<TaskLog>(
            _sql.Get("Scheduling.Select.TaskLog.ByName"),
            new { Name = taskName, Cid = companyId, Month = targetMonth });
        return rows.ToList();
    }

    /// <summary>
    /// 完成任务日志 — 更新汇总统计和完成状态
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="totalCount">总处理条数</param>
    /// <param name="successCount">成功条数</param>
    /// <param name="failCount">失败条数</param>
    /// <param name="warningCount">警告条数</param>
    /// <param name="summary">执行摘要</param>
    /// <param name="ct">取消令牌</param>
    public async Task CompleteAsync(Guid id, int totalCount, int successCount, int failCount, int warningCount, string? summary, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.Complete"),
            new { Id = id, Total = totalCount, Success = successCount, Fail = failCount, Warn = warningCount, Summary = summary });
    }

    /// <summary>
    /// 标记任务日志为失败状态
    /// </summary>
    /// <param name="id">任务日志 ID</param>
    /// <param name="error">错误信息</param>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 查询超时未更新的僵死任务
    /// </summary>
    /// <remarks>
    /// 使用 TimeSpan 计算超时分钟数，查询心跳时间超过阈值的任务。
    /// 用于调度系统启动时的清理恢复。
    /// </remarks>
    /// <param name="timeout">超时时间间隔</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>僵死任务列表</returns>
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

    public async Task<bool> ForceCompleteAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var affected = await conn.ExecuteAsync(
            _sql.Get("Scheduling.Update.TaskLog.ForceCompleteById"),
            new { Id = id });
        return affected > 0;
    }

    public async Task CompleteByNameAsync(string jobName, Guid companyId, string month, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.CompleteByName"),
            new { Status = "Completed", Now = ChinaTime.Now, Name = jobName, Cid = companyId, Month = month });
    }

    public async Task FailByNameAsync(string jobName, Guid companyId, string month, string error, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Scheduling.Update.TaskLog.FailByName"),
            new { Status = "Failed", Now = ChinaTime.Now, Error = error, Name = jobName, Cid = companyId, Month = month });
    }
}
