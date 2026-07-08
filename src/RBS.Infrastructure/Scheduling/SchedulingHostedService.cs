using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Scheduling;

/// <summary>
/// 调度宿主服务 — 每 60 秒轮询 JobScheduleExecutions，触发到期作业
///
/// 并发策略：
///   - 公司间并行（Parallel.ForEachAsync）
///   - 公司内串行（foreach），按 TargetDate 升序执行
///   - 原子抢占（UPDATE ... WHERE Status='Pending'）防止重复执行
///   - 上游失败 → 阻断下游（仅 TargetDate 靠前的失败会阻断靠后的）
///     成功/跳过/取消 均不阻断，确保依赖链自然闭合
/// </summary>
public class SchedulingHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SchedulingHostedService> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);
    private ISqlLoader? _cachedSql;

    public SchedulingHostedService(IServiceScopeFactory scopeFactory, ILogger<SchedulingHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    private ISqlLoader Sql => _cachedSql ??= ResolveSql();

    private ISqlLoader ResolveSql()
    {
        using var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ISqlLoader>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("调度引擎启动");
        await Task.Yield();

        // 启动时检测僵死任务
        await DetectStaleTasksAsync(stoppingToken);

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

    /// <summary>
    /// 处理所有到期排期：查出 Pending 且 TargetDate≤Now 的排期，
    /// 按公司分组并行执行，同一公司内串行执行。
    /// </summary>
    private async Task ProcessPendingExecutionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        // 读取所有到期（Pending 且 TargetDate≤Now）的排期
        List<(JobScheduleExecution Execution, JobSchedule Schedule)> dueItems;
        using (var conn = dbFactory.CreateConnection())
        {
            conn.Open();
            var dueExecutions = (await Dapper.SqlMapper.QueryAsync<JobScheduleExecution>(conn,
                Sql.Get("Scheduling.Select.Execution.PendingDue"),
                new { Now = ChinaTime.Now })).ToList();

            if (dueExecutions.Count == 0) return;

            // 预加载所有关联的 JobSchedule（仅活跃的）
            var scheduleIds = dueExecutions.Select(e => e.JobScheduleId).Distinct().ToList();
            var schedules = (await Dapper.SqlMapper.QueryAsync<JobSchedule>(conn,
                "SELECT * FROM [JobSchedules] WHERE [Id] IN @Ids",
                new { Ids = scheduleIds }))
                .ToDictionary(s => s.Id);

            dueItems = dueExecutions
                .Where(e => schedules.ContainsKey(e.JobScheduleId) && schedules[e.JobScheduleId].IsActive)
                .Select(e => (e, schedules[e.JobScheduleId]))
                .ToList();
        }

        if (dueItems.Count == 0) return;
        _logger.LogInformation("到期排期: {Count} 条", dueItems.Count);

        // 按公司分组：公司间并行，公司内串行
        var groups = dueItems.GroupBy(x => x.Execution.CompanyId).ToList();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(groups, parallelOptions, async (group, token) =>
        {
            // 公司内按 TargetDate 升序串行执行（先到期先执行）
            foreach (var (execution, schedule) in group.OrderBy(x => x.Execution.TargetDate))
            {
                if (token.IsCancellationRequested) break;

                var success = await ExecuteJobAsync(execution, schedule, token);
                // 执行失败 → 阻断下游任务，等重试
                if (!success) break;
            }
        });
    }

    /// <summary>执行单个排期（含原子抢占、互斥检查、步骤日志）</summary>
    /// <returns>true=执行成功, false=执行失败（上游失败时应阻断下游）</returns>
    private async Task<bool> ExecuteJobAsync(
        JobScheduleExecution execution, JobSchedule schedule, CancellationToken ct)
    {
        using var innerScope = _scopeFactory.CreateScope();
        var innerDb = innerScope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        var stepLogger = innerScope.ServiceProvider.GetRequiredService<ITaskStepLogger>();
        var jobContext = innerScope.ServiceProvider.GetRequiredService<JobExecutionContext>();
        var job = innerScope.ServiceProvider.GetRequiredService<IEnumerable<IScheduledJob>>()
            .FirstOrDefault(j => j.JobName.Equals(schedule.JobName, StringComparison.OrdinalIgnoreCase));
        using var conn = innerDb.CreateConnection();
        conn.Open();

        // 依赖检查：同一公司是否有 TargetDate 靠前的排期失败/卡住（闭合依赖链）
        var blocked = await Dapper.SqlMapper.QuerySingleAsync<int>(conn,
            Sql.Get("Scheduling.Select.Execution.BlockedByUpstream"),
            new { Cid = execution.CompanyId, TargetDate = execution.TargetDate });
        if (blocked > 0)
        {
            _logger.LogWarning("上游任务失败，跳过 {JobName}/{Month}", schedule.JobName, execution.Month);
            return false;
        }

        // 原子抢占：仅当 Status='Pending' 时才更新为 'Processing'
        var claimed = await Dapper.SqlMapper.ExecuteAsync(conn,
            Sql.Get("Scheduling.Update.Execution.Claim"),
            new { Id = execution.Id });
        if (claimed == 0) return true; // 已被其他周期抢占，不影响下游评估

        if (job == null)
        {
            _logger.LogWarning("未找到作业实现: {JobName}", schedule.JobName);
            await Dapper.SqlMapper.ExecuteAsync(conn,
                Sql.Get("Scheduling.Update.Execution.Fail"),
                new { Id = execution.Id, Reason = $"未找到作业实现: {schedule.JobName}" });
            return false; // 执行失败，阻断下游
        }

        // 互斥检查：同一任务+公司+月份是否有正在运行的日志
        var runningLogs = await Dapper.SqlMapper.QuerySingleAsync<int>(conn,
            Sql.Get("Scheduling.Select.TaskLog.Running"),
            new { Name = schedule.JobName, Cid = execution.CompanyId, Month = execution.Month });
        if (runningLogs > 0)
        {
            _logger.LogWarning("跳过 {JobName}/{CompanyId}/{Month} — 已有执行中的日志",
                schedule.JobName, execution.CompanyId, execution.Month);
            await Dapper.SqlMapper.ExecuteAsync(conn,
                Sql.Get("Scheduling.Update.Execution.Retry"),
                new { Id = execution.Id });
            return true; // 下次轮询再评估
        }

        // 创建任务日志，通过 JobExecutionContext 传递给 Job
        var taskLogId = await CreateTaskLogAsync(innerDb, schedule.JobName,
            execution.CompanyId, execution.Month, "Running", ct);
        jobContext.TaskLogId = taskLogId;

        // 步骤日志（独立捕获，失败不影响执行）
        Guid? executeStepId = null;
        try
        {
            executeStepId = await stepLogger.StartStepAsync(taskLogId,
                "Schedule.Execute", $"执行 {schedule.JobName}", null, null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "步骤日志创建失败，继续执行任务");
        }

        // 排期级心跳（独立于 TaskLog，后台每 30 秒更新）
        using var execCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var heartbeatTask = Task.Run(async () =>
        {
            while (!execCts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(30000, execCts.Token);
                    using var hbConn = innerDb.CreateConnection();
                    hbConn.Open();
                    await Dapper.SqlMapper.ExecuteAsync(hbConn,
                        Sql.Get("Scheduling.Insert.ExecutionHeartbeat.Default"),
                        new { ExecutionId = execution.Id, JobScheduleId = schedule.Id, JobName = schedule.JobName, CompanyId = execution.CompanyId, TargetMonth = execution.Month, HeartbeatAt = ChinaTime.Now });
                }
                catch { break; }
            }
        }, ct);

        try
        {
            var result = await job.ExecuteAsync(execution.CompanyId, execution.Month, ct);

            if (executeStepId.HasValue)
            {
                try { await stepLogger.CompleteStepAsync(executeStepId.Value, 1, null, ct); }
                catch { /* 步骤日志失败不影响执行 */ }
            }

            // 更新排期状态为完成
            await Dapper.SqlMapper.ExecuteAsync(conn,
                Sql.Get("Scheduling.Update.Execution.Complete"),
                new { Id = execution.Id });

            // 更新任务日志
            await UpdateTaskLogAsync(innerDb, schedule.JobName,
                execution.CompanyId, execution.Month, "Completed", null, ct);

            _logger.LogInformation("Job {JobName}/{CompanyId}/{Month} 执行成功: {Result}",
                schedule.JobName, execution.CompanyId, execution.Month, result);

            return true; // 成功，继续下游
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobName}/{CompanyId}/{Month} 执行失败",
                schedule.JobName, execution.CompanyId, execution.Month);

            if (executeStepId.HasValue)
            {
                try { await stepLogger.FailStepAsync(executeStepId.Value, ex.Message, null, ct); }
                catch { /* 步骤日志失败不影响执行 */ }
            }

            await Dapper.SqlMapper.ExecuteAsync(conn,
                Sql.Get("Scheduling.Update.Execution.Fail"),
                new { Id = execution.Id, Reason = ex.Message });

            await UpdateTaskLogAsync(innerDb, schedule.JobName,
                execution.CompanyId, execution.Month, "Failed", ex.Message, ct);

            return false; // 失败，阻断下游
        }
        finally
        {
            // 停止排期心跳
            execCts.Cancel();
            try { await heartbeatTask; } catch { /* 忽略心跳停止异常 */ }
        }
    }

    /// <summary>创建任务日志，返回 taskLogId</summary>
    private async Task<Guid> CreateTaskLogAsync(IDbConnectionFactory db, string taskName,
        Guid companyId, string month, string status, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        using var conn = db.CreateConnection(); conn.Open();
        await Dapper.SqlMapper.ExecuteAsync(conn,
            Sql.Get("Scheduling.Insert.TaskLog.Default"),
            new
            {
                Id = id,
                TaskName = taskName,
                CompanyId = companyId,
                ContractId = (Guid?)null,
                TargetMonth = month,
                TriggerType = "Schedule",
                RunMode = "Execute",
                Status = status,
                StartedAt = ChinaTime.Now,
                CreatedBy = Guid.Empty
            });
        return id;
    }

    private async Task UpdateTaskLogAsync(IDbConnectionFactory db, string taskName, Guid companyId, string month, string status, string? error, CancellationToken ct)
    {
        using var conn = db.CreateConnection(); conn.Open();
        var sqlKey = status == "Failed"
            ? "Scheduling.Update.TaskLog.FailByName"
            : "Scheduling.Update.TaskLog.CompleteByName";
        await Dapper.SqlMapper.ExecuteAsync(conn,
            Sql.Get(sqlKey),
            new { Status = status, Now = ChinaTime.Now, Error = error, Name = taskName, Cid = companyId, Month = month });
    }

    private async Task DetectStaleTasksAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var taskLogRepo = scope.ServiceProvider.GetRequiredService<ITaskLogRepository>();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            var staleTasks = await taskLogRepo.GetStaleTasksAsync(TimeSpan.FromMinutes(5), ct);

            foreach (var task in staleTasks)
            {
                _logger.LogWarning("检测到僵死任务 {TaskName}/{CompanyId}/{TargetMonth}，标记为 Stale",
                    task.TaskName, task.CompanyId, task.TargetMonth);
                await taskLogRepo.MarkStaleAsync(task.Id, ct);
            }

            if (staleTasks.Count > 0)
                _logger.LogInformation("已标记 {Count} 个僵死任务", staleTasks.Count);

            // 恢复因进程崩溃而卡在 Processing 的排期（基于独立心跳表）
            using var conn = dbFactory.CreateConnection();
            conn.Open();

            var heartbeatTimeout = ChinaTime.Now.AddMinutes(-10);
            var resetStale = await Dapper.SqlMapper.ExecuteAsync(conn,
                @"UPDATE e SET e.[Status]='Pending'
                  FROM [JobScheduleExecutions] e
                  WHERE e.[Status] IN ('Processing','Running')
                    AND NOT EXISTS (
                      SELECT 1 FROM [ExecutionHeartbeats] h
                      WHERE h.[ExecutionId]=e.[Id]
                        AND h.[HeartbeatAt]>=@Threshold
                    )",
                new { Threshold = heartbeatTimeout });
            if (resetStale > 0)
                _logger.LogWarning("恢复 {Count} 个僵死排期（心跳超时）", resetStale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "僵死任务检测异常");
        }
    }
}
