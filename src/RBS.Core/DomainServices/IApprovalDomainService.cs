namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Approval;

/// <summary>
/// 审批领域服务接口 — 审批流转、回调执行
/// </summary>
public interface IApprovalDomainService
{
    /// <summary>提交流审批请求</summary>
    void SubmitRequest(ApprovalRequest request);

    /// <summary>审批通过：校验权限 → 记录 → 判断是否终审 → 执行回调</summary>
    Task<ApprovalResult> ApproveAsync(ApprovalRequest request, Guid approverId, string? comment, CancellationToken ct = default);

    /// <summary>审批驳回：直接终审</summary>
    Task<ApprovalResult> RejectAsync(ApprovalRequest request, Guid approverId, string? comment, CancellationToken ct = default);
}

/// <summary>
/// 审批结果
/// </summary>
public class ApprovalResult
{
    public bool IsCompleted { get; set; }
    public string Action { get; set; } = "";
    public int? NextLevel { get; set; }
    public string? Status { get; set; }
}
