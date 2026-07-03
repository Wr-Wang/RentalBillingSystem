using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Infrastructure.Scheduling;

/// <summary>
/// 调度宿主服务 — 每 60 秒轮询 JobScheduleExecutions，触发到期作业
/// </summary>
public class SchedulingHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SchedulingHostedService> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

    public SchedulingHostedService(IServiceScopeFactory scopeFactory, ILogger<SchedulingHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("调度引擎启动");
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingExecutionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调度轮询异常");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("调度引擎停止");
    }

    private async Task ProcessPendingExecutionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        var jobs = scope.ServiceProvider.GetRequiredService<IEnumerable<IScheduledJob>>();
        var jobDict = jobs.ToDictionary(j => j.JobName, StringComparer.OrdinalIgnoreCase);

        var now = ChinaTime.Now;
        var windowStart = now.AddMinutes(-1);
        var windowEnd = now.AddMinutes(1);

        // 查找当前时间窗口内待执行的排期
        var allExecutions = await uow.JobScheduleExecutions.GetAllAsync(ct);
        var pending = allExecutions
            .Where(e => e.Status == "Pending"
                     && e.TargetDate >= windowStart
                     && e.TargetDate <= windowEnd)
            .ToList();

        if (pending.Count == 0) return;

        foreach (var execution in pending)
        {
            var jobSchedule = await uow.JobSchedules.GetByIdAsync(execution.JobScheduleId, ct);
            if (jobSchedule == null || !jobSchedule.IsActive) continue;

            if (!jobDict.TryGetValue(jobSchedule.JobName, out var job))
            {
                _logger.LogWarning("未找到作业实现: {JobName}", jobSchedule.JobName);
                continue;
            }

            // 互斥检查：同一任务+公司+月份是否有正在运行的日志
            var existingLogs = await GetRunningLogsAsync(dbFactory, jobSchedule.JobName, execution.CompanyId, execution.Month, ct);
            if (existingLogs > 0)
            {
                _logger.LogWarning("跳过 {JobName}/{CompanyId}/{Month} — 已有执行中的日志", jobSchedule.JobName, execution.CompanyId, execution.Month);
                continue;
            }

            try
            {
                // 创建执行日志
                await CreateTaskLogAsync(dbFactory, jobSchedule.JobName, execution.CompanyId, execution.Month, "Running", ct);

                var result = await job.ExecuteAsync(execution.CompanyId, execution.Month, ct);

                // 更新为完成
                await UpdateTaskLogAsync(dbFactory, jobSchedule.JobName, execution.CompanyId, execution.Month, "Completed", null, ct);

                // 更新执行状态
                execution.MarkAdjusted();
                await uow.CommitAsync(ct);

                _logger.LogInformation("Job {JobName} 执行成功: {Result}", jobSchedule.JobName, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job {JobName} 执行失败", jobSchedule.JobName);
                await UpdateTaskLogAsync(dbFactory, jobSchedule.JobName, execution.CompanyId, execution.Month, "Failed", ex.Message, ct);
            }
        }
    }

    private async Task<int> GetRunningLogsAsync(IDbConnectionFactory db, string taskName, Guid companyId, string month, CancellationToken ct)
    {
        using var conn = db.CreateConnection(); conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM ScheduledTaskLogs WHERE TaskName=@p0 AND CompanyId=@p1 AND TargetMonth=@p2 AND Status='Running'";
        cmd.Parameters.Add(new { });
        // 用 Dapper 替代
        return await Dapper.SqlMapper.QuerySingleAsync<int>(conn,
            "SELECT COUNT(1) FROM ScheduledTaskLogs WHERE TaskName=@Name AND CompanyId=@Cid AND TargetMonth=@Month AND Status='Running'",
            new { Name = taskName, Cid = companyId, Month = month });
    }

    private async Task CreateTaskLogAsync(IDbConnectionFactory db, string taskName, Guid companyId, string month, string status, CancellationToken ct)
    {
        using var conn = db.CreateConnection(); conn.Open();
        await Dapper.SqlMapper.ExecuteAsync(conn,
            "INSERT INTO ScheduledTaskLogs (Id, TaskName, CompanyId, TargetMonth, Status, StartedAt, HeartbeatAt, CreatedBy, CreatedAt) " +
            "VALUES (@Id, @Name, @Cid, @Month, @Status, @Now, @Now, @Empty, @Now)",
            new { Id = Guid.NewGuid(), Name = taskName, Cid = companyId, Month = month, Status = status, Now = ChinaTime.Now, Empty = Guid.Empty });
    }

    private async Task UpdateTaskLogAsync(IDbConnectionFactory db, string taskName, Guid companyId, string month, string status, string? error, CancellationToken ct)
    {
        using var conn = db.CreateConnection(); conn.Open();
        await Dapper.SqlMapper.ExecuteAsync(conn,
            "UPDATE ScheduledTaskLogs SET Status=@Status, CompletedAt=@Now, ErrorMessage=@Error " +
            "WHERE TaskName=@Name AND CompanyId=@Cid AND TargetMonth=@Month AND Status='Running'",
            new { Status = status, Now = ChinaTime.Now, Error = error, Name = taskName, Cid = companyId, Month = month });
    }
}
