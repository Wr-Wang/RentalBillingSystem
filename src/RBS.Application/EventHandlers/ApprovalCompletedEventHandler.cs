using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 审批完成事件处理器 — 审批通过/驳回后执行业务回调
/// </summary>
public class ApprovalCompletedEventHandler : IEventHandler<ApprovalCompletedEvent>
{
    private readonly IImportService _importService;
    private readonly IUnitOfWork _uow;

    public ApprovalCompletedEventHandler(IImportService importService, IUnitOfWork uow)
    {
        _importService = importService;
        _uow = uow;
    }

    public async Task HandleAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        switch (@event.TargetEntityType)
        {
            case "Import":
                if (@event.Action == "Approved")
                {
                    await _importService.ExecuteApprovedImportAsync(@event.TargetEntityId, ct);
                }
                else if (@event.Action == "Rejected")
                {
                    // 审批驳回 → 更新批次状态为 Rejected
                    var batch = await _uow.ImportBatches.GetByIdAsync(@event.TargetEntityId, ct);
                    if (batch != null && batch.Status == "PendingApproval")
                    {
                        batch.Status = "Rejected";
                        await _uow.CommitAsync(ct);
                    }
                }
                break;

            default:
                await Task.CompletedTask;
                break;
        }
    }
}
