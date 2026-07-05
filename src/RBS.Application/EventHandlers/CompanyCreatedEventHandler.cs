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

        // 查询全局模板（CompanyId IS NULL）
        var templates = await conn.QueryAsync<dynamic>(
            "SELECT JobName, CronExpression, Description FROM JobSchedules WHERE CompanyId IS NULL AND IsActive = 1");

        foreach (var t in templates)
        {
            await conn.ExecuteAsync(
                @"INSERT INTO JobSchedules (Id, JobName, CronExpression, IsActive, CompanyId, Description, CreatedBy, CreatedAt)
                  VALUES (NEWID(), @Name, @Cron, 1, @Cid, @Desc, @CBy, GETUTCDATE())",
                new
                {
                    Name = (string)t.JobName,
                    Cron = (string)t.CronExpression,
                    Desc = (string?)t.Description ?? "",
                    Cid = @event.CompanyId,
                    CBy = Guid.Empty
                });
        }
    }
}
