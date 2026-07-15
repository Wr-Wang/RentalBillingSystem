using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 公司创建事件处理器 — 新公司注册时自动从全局模板（CompanyId IS NULL）
/// 复制所有启用的 JobSchedule 配置到新公司，实现开箱即用的调度任务初始化
/// </summary>
public class CompanyCreatedEventHandler : IEventHandler<CompanyCreatedEvent>
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="sql">SQL 加载器</param>
    public CompanyCreatedEventHandler(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    /// <summary>
    /// 处理公司创建事件 — 从全局模板复制任务调度配置到新公司
    /// </summary>
    public async Task HandleAsync(CompanyCreatedEvent @event, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        // 从全局模板（CompanyId IS NULL）复制任务调度配置到新公司
        var templates = await conn.QueryAsync<dynamic>(
            _sql.Get("Scheduling.Select.JobSchedule.GlobalTemplates"));

        foreach (var t in templates)
        {
            await conn.ExecuteAsync(
                _sql.Get("Scheduling.Insert.JobSchedule.FromTemplate"),
                new
                {
                    JobName = (string)t.JobName,
                    ScheduleType = (string)t.ScheduleType,
                    Hour = (int)t.Hour,
                    Minute = (int)t.Minute,
                    DayOfMonth = (int?)t.DayOfMonth,
                    TemplateCode = (string?)t.TemplateCode,
                    IsActive = (bool)t.IsActive,
                    Desc = (string?)t.Description ?? "",
                    Cid = @event.CompanyId,
                    CBy = Guid.Empty
                });
        }
    }
}
