namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

/// <summary>
/// 审批领域服务 — 多级审批流转
/// 领域服务只做编排，领域事件由聚合根自身触发
/// </summary>
public class ApprovalDomainService : IApprovalDomainService
{
    public void SubmitRequest(ApprovalRequest request)
    {
        request.Submit();
    }

    public async Task<ApprovalResult> ApproveAsync(ApprovalRequest request, Guid approverId, string? comment, CancellationToken ct = default)
    {
        // 记录审批操作（聚合根内部管理记录）
        request.AddRecord(approverId, "Approved", comment);

        // 判断是否为最后一个级别
        if (request.CurrentLevel >= request.MaxLevel)
        {
            // 终审通过 — 聚合根内部会触发 ApprovalCompletedEvent
            request.CompleteApproval("Approved");

            return new ApprovalResult
            {
                IsCompleted = true,
                Action = "Approved",
                Status = "Approved"
            };
        }

        // 进入下一级审批
        request.AdvanceLevel();

        return new ApprovalResult
        {
            IsCompleted = false,
            Action = "Approved",
            NextLevel = request.CurrentLevel,
            Status = "Pending"
        };
    }

    public async Task<ApprovalResult> RejectAsync(ApprovalRequest request, Guid approverId, string? comment, CancellationToken ct = default)
    {
        // 记录驳回操作
        request.AddRecord(approverId, "Rejected", comment);

        // 驳回直接终审 — 聚合根内部会触发 ApprovalCompletedEvent
        request.CompleteApproval("Rejected");

        return new ApprovalResult
        {
            IsCompleted = true,
            Action = "Rejected",
            Status = "Rejected"
        };
    }
}
