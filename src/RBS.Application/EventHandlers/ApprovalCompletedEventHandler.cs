using RBS.Core.Common;
using RBS.Core.Entities.Base;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 审批完成事件处理器 — 审批通过/驳回后执行业务回调
/// </summary>
public class ApprovalCompletedEventHandler : IEventHandler<ApprovalCompletedEvent>
{
    public async Task HandleAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        // 根据 TargetEntityType 分发到不同业务处理
        // 目前为预留 — 具体业务对接可在后续扩展
        await Task.CompletedTask;
    }
}
