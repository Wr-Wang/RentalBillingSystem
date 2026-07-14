namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

/// <summary>
/// 审批领域服务 — 多级审批流转。
/// 实现 IApprovalDomainService 接口，编排审批提交流、逐级审批通过、
/// 驳回等业务流程。领域服务只做流程编排，领域事件（如 ApprovalCompletedEvent）
/// 由聚合根自身触发，服务不直接处理事件。
/// </summary>
public class ApprovalDomainService : IApprovalDomainService
{
    /// <summary>
    /// 提交审批请求。
    /// 调用聚合根的 Submit 方法，将请求初始化为待审批状态。
    /// </summary>
    /// <param name="request">待提交的审批请求聚合根</param>
    public void SubmitRequest(ApprovalRequest request)
    {
        request.Submit();
    }

    /// <summary>
    /// 审批通过：记录审批操作 → 判断是否为终审 → 执行相应流转。
    /// 终审时调用 CompleteApproval 完成审批并触发领域事件，
    /// 非终审时调用 AdvanceLevel 进入下一级审批。
    /// </summary>
    /// <param name="request">审批请求聚合根</param>
    /// <param name="approverId">审批人用户 ID</param>
    /// <param name="comment">审批意见备注，可为空</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批结果，包含是否终审、下一步级别等信息</returns>
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

    /// <summary>
    /// 审批驳回：直接终审，无需继续流转。
    /// 记录驳回操作后调用 CompleteApproval 将审批置为"Rejected"状态，
    /// 聚合根内部会触发 ApprovalCompletedEvent 供事件处理器执行回调。
    /// </summary>
    /// <param name="request">审批请求聚合根</param>
    /// <param name="approverId">驳回人用户 ID</param>
    /// <param name="comment">驳回原因备注，可为空</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>终审驳回的审批结果</returns>
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
