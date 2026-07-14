using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Infrastructure.Scheduling;

/// <summary>
/// 调度生成器 — 每小时扫描一次 JobSchedules，按 ScheduleType/Hour/Minute/DayOfMonth 生成执行记录
/// </summary>
/// <remarks>
/// 工作原理：
/// <list type="bullet">
///   <item><description>每小时运行一次 GeneratePendingExecutionsAsync</description></item>
///   <item><description>查询所有活跃的 JobSchedules，计算每个排期的下次运行时间</description></item>
///   <item><description>只生成未来 1 小时窗口内的执行记录</description></item>
///   <item><description>避免重复生成：检查同一 JobScheduleId+Month 是否已存在执行记录</description></item>
///   <item><description>支持 Daily（每日指定时分）和 Monthly（每月指定日时分）两种频率</description></item>
///   <item><description>Monthly 模式的天数自动限制在 1~28 之间，避免 2 月/30 日等无效日期</description></item>
/// </list>
/// 设计模式：BackgroundService 定时任务生成器。
/// </remarks>
public class JobScheduleGenerator : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobScheduleGenerator> _logger;
    /// <summary>生成间隔（1 小时）</summary>
    private static readonly TimeSpan GenerateInterval = TimeSpan.FromHours(1);

    /// <summary>
    /// 初始化调度生成器
    /// </summary>
    /// <param name="scopeFactory">服务作用域工厂</param>
    /// <param name="logger">日志记录器</param>
    public JobScheduleGenerator(IServiceScopeFactory scopeFactory, ILogger<JobScheduleGenerator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobScheduleGenerator 启动");
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await GeneratePendingExecutionsAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "JobScheduleGenerator 异常"); }
            await Task.Delay(GenerateInterval, stoppingToken);
        }
        _logger.LogInformation("JobScheduleGenerator 停止");
    }

    /// <summary>
    /// 生成未来一小时内到期的排期执行记录
    /// </summary>
    /// <remarks>
    /// 生成逻辑：
    /// <list type="bullet">
    ///   <item><description>查询所有 IsActive=1 的 JobSchedules</description></item>
    ///   <item><description>计算每个排期的下次运行时间（基于 ScheduleType/Hour/Minute/DayOfMonth）</description></item>
    ///   <item><description>如果下次运行时间在未来 1 小时内则生成执行记录（JobScheduleExecutions）</description></item>
    ///   <item><description>去重检查：同一排期+月份不能有重复记录</description></item>
    ///   <item><description>单个排期生成失败仅记录警告，不影响其他排期</description></item>
    /// </list>
    /// </remarks>
    /// <param name="ct">取消令牌</param>
    private async Task GeneratePendingExecutionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using var conn = db.CreateConnection();
        conn.Open();

        var schedules = await conn.QueryAsync<dynamic>(
            "SELECT Id, JobName, ScheduleType, Hour, Minute, DayOfMonth, CompanyId " +
            "FROM JobSchedules WHERE IsActive = 1");

        var now = ChinaTime.Now;
        var windowEnd = now.AddHours(1);

        foreach (var s in schedules)
        {
            try
            {
                DateTime? nextRun = GetNextRunTime(s, now);
                if (nextRun == null || nextRun > windowEnd) continue;

                var nr = nextRun.Value; var targetMonth = $"{nr.Year}-{nr.Month:D2}";

                var exists = await conn.QuerySingleAsync<int>(
                    "SELECT COUNT(1) FROM JobScheduleExecutions WHERE JobScheduleId = @Id AND Month = @Month",
                    new { Id = (Guid)s.Id, Month = targetMonth });
                if (exists > 0) continue;

                await conn.ExecuteAsync(
                    @"INSERT INTO JobScheduleExecutions (Id, JobScheduleId, CompanyId, TargetDate, OriginalDate, Month, Status, IsAdjusted, IsCustom, CreatedBy, CreatedAt)
                      VALUES (@Id, @SId, @CId, @Date, @Date, @Month, 'Pending', 0, 0, @CBy, @Now)",
                    new
                    {
                        Id = Guid.NewGuid(), SId = (Guid)s.Id, CId = (Guid)s.CompanyId,
                        Date = nextRun.Value.ToUniversalTime(), Month = targetMonth,
                        CBy = Guid.Empty, Now = DateTime.UtcNow
                    });

                Console.WriteLine("生成排期: " + (string)s.JobName + " -> " + nr.ToString("yyyy-MM-dd HH:mm"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "排期生成失败: {JobName}", (string)s.JobName);
            }
        }
    }

    /// <summary>
    /// 计算排期的下次执行时间
    /// </summary>
    /// <remarks>
    /// Daily 类型：计算当天指定时分，如果已过则返回次日
    /// Monthly 类型：从当月开始往后搜索最多 12 个月，找到指定日期的下一个有效日期
    /// 安全措施：月天数限制在 1~28，避免 2 月 30 日等问题
    /// </remarks>
    /// <param name="schedule">排期动态对象（含 ScheduleType, Hour, Minute, DayOfMonth 属性）</param>
    /// <param name="now">当前时间参照</param>
    /// <returns>下次执行时间，无效配置返回 null</returns>
    private static DateTime? GetNextRunTime(dynamic schedule, DateTime now)
    {
        int hour = (int)schedule.Hour;
        int minute = (int)schedule.Minute;

        if (hour < 0 || hour > 23 || minute < 0 || minute > 59) return null;

        string type = ((string)schedule.ScheduleType)?.ToLower() ?? "monthly";

        if (type == "daily")
        {
            var runTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            return runTime <= now ? runTime.AddDays(1) : runTime;
        }

        // Monthly
        int day = schedule.DayOfMonth != null ? (int)schedule.DayOfMonth : 1;
        day = Math.Max(1, Math.Min(day, 28)); // 安全范围

        for (int m = 0; m <= 12; m++)
        {
            try
            {
                var t = new DateTime(now.Year, now.Month, 1, hour, minute, 0).AddMonths(m);
                var maxDay = DateTime.DaysInMonth(t.Year, t.Month);
                t = new DateTime(t.Year, t.Month, Math.Min(day, maxDay), hour, minute, 0);
                if (t > now) return t;
            }
            catch { continue; }
        }
        return null;
    }
}
