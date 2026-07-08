using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 公司创建事件处理器 — 自动从全局模板复制 JobSchedule
/// </summary>
public class CompanyCreatedEventHandler : IEventHandler<CompanyCreatedEvent>
{
    private readonly IDbConnectionFactory _db;

    public CompanyCreatedEventHandler(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task HandleAsync(CompanyCreatedEvent @event, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        // 从全局模板（CompanyId IS NULL）复制任务调度配置到新公司
        var templates = await conn.QueryAsync<dynamic>(
            @"SELECT JobName, ScheduleType, Hour, Minute, DayOfMonth, TemplateCode,
                     IsActive, Description
              FROM JobSchedules WHERE CompanyId IS NULL AND IsActive = 1");

        foreach (var t in templates)
        {
            await conn.ExecuteAsync(
                @"INSERT INTO JobSchedules
                    (Id, JobName, ScheduleType, Hour, Minute, DayOfMonth,
                     TemplateCode, IsActive, CompanyId, Description, CreatedBy, CreatedAt)
                  VALUES
                    (NEWID(), @JobName, @ScheduleType, @Hour, @Minute, @DayOfMonth,
                     @TemplateCode, @IsActive, @Cid, @Desc, @CBy, GETUTCDATE())",
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
