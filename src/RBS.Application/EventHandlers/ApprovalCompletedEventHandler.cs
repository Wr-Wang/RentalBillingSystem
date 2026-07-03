using System.Text.RegularExpressions;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 审批完成事件处理器 — 审批通过/驳回后执行业务回调 + 通知相关人员
/// </summary>
public class ApprovalCompletedEventHandler : IEventHandler<ApprovalCompletedEvent>
{
    private readonly IImportService _importService;
    private readonly IContractService _contractService;
    private readonly IRenewalService _renewalService;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public ApprovalCompletedEventHandler(
        IImportService importService,
        IContractService contractService,
        IRenewalService renewalService,
        IUnitOfWork uow,
        INotificationService notificationService)
    {
        _importService = importService;
        _contractService = contractService;
        _renewalService = renewalService;
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task HandleAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        // 1. 业务回调
        await ExecuteBusinessCallbacksAsync(@event, ct);

        // 2. 通知相关人员
        await SendNotificationsAsync(@event, ct);
    }

    private async Task ExecuteBusinessCallbacksAsync(ApprovalCompletedEvent @event, CancellationToken ct)
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
                    var batch = await _uow.ImportBatches.GetByIdAsync(@event.TargetEntityId, ct);
                    if (batch != null && batch.Status == "PendingApproval")
                    {
                        batch.Status = "Rejected";
                        await _uow.CommitAsync(ct);
                    }
                }
                break;

            case "Contract":
                if (@event.Action == "Approved")
                {
                    var request = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
                    if (request?.Description == null) break;

                    // 合同终止
                    if (request.Title.StartsWith("[合同终止]"))
                    {
                        var contract = await _uow.Contracts.GetByIdAsync(@event.TargetEntityId, ct);
                        if (contract != null && contract.StatusCode != "Terminated")
                        {
                            contract.Terminate(request.Description);
                            await _uow.CommitAsync(ct);
                        }
                    }
                    else
                    {
                        // 租金调整（现有逻辑）
                        var match = Regex.Match(request.Description, @"→\s*¥([\d,]+)");
                        if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var newAmount))
                        {
                            await _contractService.AdjustRentAsync(@event.TargetEntityId, newAmount, ct);
                        }
                    }
                }
                break;

            case "ContractRenewal":
                if (@event.Action == "Approved")
                {
                    await _renewalService.ExecuteRenewalAsync(@event.TargetEntityId, ct);
                }
                else if (@event.Action == "Rejected")
                {
                    var renewal = await _uow.RenewalRequests.GetByIdAsync(@event.TargetEntityId, ct);
                    if (renewal != null)
                    {
                        await _uow.ExecuteSqlRawAsync(
                            "UPDATE RenewalRequests SET Status = 'Rejected', UpdatedAt = GETUTCDATE() WHERE Id = @Id AND Status = 'PendingApproval'",
                            new object[] { renewal.Id }, ct);
                    }
                }
                break;
        }
    }

    private async Task SendNotificationsAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        var request = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
        if (request == null) return;

        if (@event.Action == "Approved")
        {
            // 通知提交人
            await _notificationService.NotifySubmitterAsync(
                @event.ApprovalRequestId, $"{request.Title} 已通过", null, ct);

            // 通知全部审批参与人
            await _notificationService.NotifyAllParticipantsAsync(
                @event.ApprovalRequestId, $"{request.Title} 已通过", null, ct);
        }
        else if (@event.Action == "Rejected")
        {
            // 找驳回记录中的审批意见
            var records = (await _uow.ApprovalRequests.GetByIdWithRecordsAsync(@event.ApprovalRequestId, ct))?.Records;
            var rejectRecord = records?.FirstOrDefault(r => r.Action == "Rejected");
            var reason = rejectRecord?.Comment;
            var content = reason != null ? $"原因：{reason}" : null;

            // 通知提交人
            await _notificationService.NotifySubmitterAsync(
                @event.ApprovalRequestId, $"{request.Title} 已驳回", content, ct);
        }
    }
}
