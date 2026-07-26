using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 公司创建事件处理器 — 新公司注册时自动从全局模板（CompanyId IS NULL）
/// 复制所有启用的 JobSchedule 配置到新公司，实现开箱即用的调度任务初始化。
/// DDD: 事件处理器只做编排，通过 IUnitOfWork 仓储接口访问数据。
/// </summary>
public class CompanyCreatedEventHandler : IEventHandler<CompanyCreatedEvent>
{
    private readonly IUnitOfWork _uow;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="uow">工作单元（通过仓储接口访问数据，不直接依赖基础设施）</param>
    public CompanyCreatedEventHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// 处理公司创建事件 — 从全局模板复制任务调度配置到新公司
    /// </summary>
    public async Task HandleAsync(CompanyCreatedEvent @event, CancellationToken ct)
    {
        // 1. 通过仓储获取全局模板（CompanyId = Guid.Empty 表示全局模板）
        var allSchedules = await _uow.JobSchedules.GetAllAsync(ct);
        var templates = allSchedules.Where(s => s.CompanyId == Guid.Empty && s.IsActive);

        // 2. 为每个模板创建新公司的排期副本（通过领域实体构造 + 仓储写入）
        foreach (var t in templates)
        {
            var schedule = new JobSchedule(
                t.JobName, t.ScheduleType, t.Hour, t.Minute, @event.CompanyId, t.DayOfMonth);

            schedule.SetCreated(Guid.Empty, ChinaTime.Now, null, null);
            schedule.SetDescription(t.Description);
            schedule.SetTemplateCode(t.TemplateCode);
            if (!t.IsActive) schedule.Deactivate();

            await _uow.JobSchedules.AddAsync(schedule, ct);
        }

        await _uow.CommitAsync(ct);
    }
}
