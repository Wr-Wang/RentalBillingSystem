using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 审批推进事件处理器 — 通知下一级审批人
/// </summary>
public class ApprovalLevelAdvancedEventHandler : IEventHandler<ApprovalLevelAdvancedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _uow;

    public ApprovalLevelAdvancedEventHandler(INotificationService notificationService, IUnitOfWork uow)
    {
        _notificationService = notificationService;
        _uow = uow;
    }

    public async Task HandleAsync(ApprovalLevelAdvancedEvent @event, CancellationToken ct)
    {
        var request = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
        if (request == null) return;

        // NextLevel 是计划推进到的级别（即待审批级别），
        // 已完成的是 NextLevel - 1（SQL 已递增 CurrentLevel，但这里用事件参数更可靠）
        var completedLevel = @event.NextLevel - 1;
        var title = $"{request.Title} 已通过第 {completedLevel} 级，待您审批";
        await _notificationService.NotifyApproversAsync(
            @event.ApprovalRequestId, @event.NextLevel, title, null, ct);
    }
}
