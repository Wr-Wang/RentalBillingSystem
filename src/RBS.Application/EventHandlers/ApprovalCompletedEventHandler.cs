using System.Text.RegularExpressions;
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
    private readonly IContractService _contractService;
    private readonly IRenewalService _renewalService;
    private readonly IUnitOfWork _uow;

    public ApprovalCompletedEventHandler(IImportService importService, IContractService contractService, IRenewalService renewalService, IUnitOfWork uow)
    {
        _importService = importService;
        _contractService = contractService;
        _renewalService = renewalService;
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

            case "Contract":
                if (@event.Action == "Approved")
                {
                    var request = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
                    if (request?.Description != null)
                    {
                        // Description 格式: "月租金 ¥5,200 → ¥6,000，差额：+¥800，..."
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
                    // 续签审批通过 → 执行续签
                    await _renewalService.ExecuteRenewalAsync(@event.TargetEntityId, ct);
                }
                else if (@event.Action == "Rejected")
                {
                    // 续签驳回 → 更新状态
                    var renewal = await _uow.RenewalRequests.GetByIdAsync(@event.TargetEntityId, ct);
                    if (renewal != null)
                    {
                        // 使用反射调用私有方法 — 因为 RenewalRequest.Reject() 有校验
                        // 直接用仓储加载后通过 UpdateAsync 更新
                        await _uow.ExecuteSqlRawAsync(
                            "UPDATE RenewalRequests SET Status = 'Rejected', UpdatedAt = GETUTCDATE() WHERE Id = @Id AND Status = 'PendingApproval'",
                            new object[] { renewal.Id }, ct);
                    }
                }
                break;

            default:
                await Task.CompletedTask;
                break;
        }
    }
}
