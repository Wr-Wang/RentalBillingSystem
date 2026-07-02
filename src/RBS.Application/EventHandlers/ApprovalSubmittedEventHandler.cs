using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 审批已提交事件处理器 — 通知第一级审批人
/// </summary>
public class ApprovalSubmittedEventHandler : IEventHandler<ApprovalSubmittedEvent>
{
    private readonly INotificationService _notificationService;

    public ApprovalSubmittedEventHandler(INotificationService notificationService)
        => _notificationService = notificationService;

    public async Task HandleAsync(ApprovalSubmittedEvent @event, CancellationToken ct)
    {
        var title = $"{@event.Title} 已提交，待您审批";
        await _notificationService.NotifyApproversAsync(
            @event.ApprovalRequestId, level: 1, title, null, ct);
    }
}
