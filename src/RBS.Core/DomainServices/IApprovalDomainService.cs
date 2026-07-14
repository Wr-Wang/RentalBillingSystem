namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Approval;

/// <summary>
/// 审批领域服务接口 — 审批流转、回调执行。
/// 定义审批请求的提交流、多级审批通过、驳回等核心业务流程契约，
/// 以及审批结果的数据结构。
/// </summary>
public interface IApprovalDomainService
{
    /// <summary>
    /// 提交审批请求。
    /// 将审批请求的初始状态置为待审批，触发审批流转起点。
    /// </summary>
    /// <param name="request">待提交的审批请求聚合根</param>
    void SubmitRequest(ApprovalRequest request);

    /// <summary>
    /// 审批通过：校验权限 → 记录审批记录 → 判断是否为终审 → 执行回调。
    /// 若当前级别已达到最大级别，则终审通过并触发完成事件；
    /// 否则进入下一级审批。
    /// </summary>
    /// <param name="request">审批请求聚合根</param>
    /// <param name="approverId">审批人用户 ID</param>
    /// <param name="comment">审批意见备注，可为空</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批结果，包含是否终审、执行动作和下一级等信息</returns>
    Task<ApprovalResult> ApproveAsync(ApprovalRequest request, Guid approverId, string? comment, CancellationToken ct = default);

    /// <summary>
    /// 审批驳回：直接终审，无需继续流转。
    /// 记录驳回操作后直接将审批状态置为"Rejected"，触发完成事件。
    /// </summary>
    /// <param name="request">审批请求聚合根</param>
    /// <param name="approverId">驳回人用户 ID</param>
    /// <param name="comment">驳回原因备注，可为空</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批结果，Action 为"Rejected"</returns>
    Task<ApprovalResult> RejectAsync(ApprovalRequest request, Guid approverId, string? comment, CancellationToken ct = default);
}

/// <summary>
/// 审批结果。
/// 封装审批操作后的业务状态，包括是否终审完成、执行的动作类型、
/// 下一级编号以及当前审批状态。
/// </summary>
public class ApprovalResult
{
    /// <summary>是否终审完成（终审通过或驳回后为 true）</summary>
    public bool IsCompleted { get; set; }
    /// <summary>执行的动作类型（"Approved"或"Rejected"）</summary>
    public string Action { get; set; } = "";
    /// <summary>下一级审批级别编号（非终审时有效）</summary>
    public int? NextLevel { get; set; }
    /// <summary>审批请求当前状态（"Approved"/"Rejected"/"Pending"）</summary>
    public string? Status { get; set; }
}
