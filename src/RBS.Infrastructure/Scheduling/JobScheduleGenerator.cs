using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Infrastructure.Scheduling;

/// <summary>
/// 调度生成器 — 每小时扫描一次 JobSchedules
/// 按 ScheduleType/Hour/Minute/DayOfMonth 生成执行记录
/// </summary>
public class JobScheduleGenerator : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobScheduleGenerator> _logger;
    private static readonly TimeSpan GenerateInterval = TimeSpan.FromHours(1);

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
                var nextRun = GetNextRunTime(s, now);
                if (nextRun == null || nextRun > windowEnd) continue;

                var nr = nextRun!.Value; var targetMonth = $"{nr.Year}-{nr.Month:D2}";

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
