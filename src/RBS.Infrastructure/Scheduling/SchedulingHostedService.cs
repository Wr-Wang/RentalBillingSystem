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
    /// <summary>服务作用域工厂，用于在轮询时创建独立 DI 作用域</summary>
    private readonly IServiceScopeFactory _scopeFactory;
    /// <summary>日志记录器</summary>
    private readonly ILogger<SchedulingHostedService> _logger;
    /// <summary>轮询间隔（60 秒）</summary>
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);
    /// <summary>缓存 SqlLoader 实例（因运行在 Singleton 中，懒加载避免启动时依赖未就绪）</summary>
    private ISqlLoader? _cachedSql;

    /// <summary>
    /// 初始化调度宿主服务
    /// </summary>
    /// <param name="scopeFactory">服务作用域工厂</param>
    /// <param name="logger">日志记录器</param>
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
    /// <remarks>
    /// 执行策略：
    /// <list type="bullet">
    ///   <item><description>先查出所有到期执行记录及其关联的活跃 JobSchedule</description></item>
    ///   <item><description>按 CompanyId 分组，公司间使用 Parallel.ForEachAsync 并行</description></item>
    ///   <item><description>同一公司内按 TargetDate 升序串行，确保先到期先执行</description></item>
    ///   <item><description>上游执行失败会阻断下游（返回 false 时 break）</description></item>
    /// </list>
    /// </remarks>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 执行单个排期（含原子抢占、互斥检查、步骤日志）
    /// </summary>
    /// <remarks>
    /// 执行流程：
    /// <list type="bullet">
    ///   <item><description>依赖检查：查询同一公司是否有 TargetDate 靠前的失败排期（阻断依赖链）</description></item>
    ///   <item><description>原子抢占：UPDATE ... WHERE Status='Pending' 防止重复执行（乐观锁机制）</description></item>
    ///   <item><description>互斥检查：同一任务+公司+月份是否有正在运行的任务日志</description></item>
    ///   <item><description>创建任务日志和步骤日志，通过 JobExecutionContext 传递给具体 Job</description></item>
    ///   <item><description>启动排期级心跳线程（每 30 秒更新），用于进程崩溃恢复</description></item>
    ///   <item><description>执行完成后更新排期状态和任务日志</description></item>
    ///   <item><description>异常时记录错误并标记失败，finally 中停止心跳</description></item>
    /// </list>
    /// </remarks>
    /// <param name="execution">排期执行记录</param>
    /// <param name="schedule">排期定义</param>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 创建任务日志
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="taskName">任务名称</param>
    /// <param name="companyId">公司 ID</param>
    /// <param name="month">目标月份</param>
    /// <param name="status">初始状态</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新建任务日志的 ID</returns>
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

    /// <summary>
    /// 更新任务日志状态（完成或失败）
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="taskName">任务名称</param>
    /// <param name="companyId">公司 ID</param>
    /// <param name="month">目标月份</param>
    /// <param name="status">目标状态（"Completed" 或 "Failed"）</param>
    /// <param name="error">失败时的错误信息</param>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 启动时检测并恢复僵死任务和排期
    /// </summary>
    /// <remarks>
    /// 两个步骤：
    /// <list type="bullet">
    ///   <item><description>使用 GetStaleTasksAsync 查询心跳超时的任务日志并标记为 Stale</description></item>
    ///   <item><description>基于 ExecutionHeartbeats 表，恢复因进程崩溃而卡在 Processing/Running 的排期为 Pending</description></item>
    /// </list>
    /// 心跳超时阈值：任务日志 5 分钟，排期心跳 10 分钟。
    /// </remarks>
    /// <param name="ct">取消令牌</param>
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
