namespace RBS.Core.Entities.Approval;

using RBS.Core.Entities.Base;

/// <summary>
/// 审批操作记录 — AssociationEntity（关联表，不做修改追踪）
/// 记录每一级审批的具体操作内容（通过/驳回）、审批人及审批意见
/// </summary>
public class ApprovalRecord : AssociationEntity
{
    /// <summary>
    /// 所属审批请求标识
    /// </summary>
    public Guid ApprovalRequestId { get; private set; }

    /// <summary>
    /// 当前审批级别序号（从 1 开始）
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// 审批人标识
    /// </summary>
    public Guid ApproverId { get; private set; }

    /// <summary>
    /// 审批动作（Approved = 通过，Rejected = 驳回）
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// 审批意见
    /// </summary>
    public string? Comment { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ApprovalRecord() { }

    /// <summary>
    /// 创建审批操作记录
    /// 创建时间和创建人由基类 <see cref="AssociationEntity.SetCreated"/> 设置
    /// </summary>
    /// <param name="approvalRequestId">所属审批请求标识</param>
    /// <param name="level">当前审批级别</param>
    /// <param name="approverId">审批人标识</param>
    /// <param name="action">审批动作（Approved / Rejected）</param>
    /// <param name="comment">审批意见（可选）</param>
    internal ApprovalRecord(Guid approvalRequestId, int level, Guid approverId, string action, string? comment = null)
    {
        ApprovalRequestId = approvalRequestId;
        Level = level;
        ApproverId = approverId;
        Action = action;
        Comment = comment?.Trim();
    }

    /// <summary>
    /// 审批操作记录的工厂方法
    /// </summary>
    /// <param name="approvalRequestId">所属审批请求标识</param>
    /// <param name="level">当前审批级别</param>
    /// <param name="approverId">审批人标识</param>
    /// <param name="action">审批动作（Approved = 通过，Rejected = 驳回）</param>
    /// <param name="comment">审批意见（可选）</param>
    /// <param name="createdBy">创建人标识（审批人自身）</param>
    /// <param name="createdAt">创建时间</param>
    /// <returns>已设置审计信息的审批操作记录</returns>
    /// <exception cref="ArgumentException">当审批动作为空或无效时抛出</exception>
    public static ApprovalRecord Create(
        Guid approvalRequestId, int level, Guid approverId,
        string action, string? comment, Guid createdBy, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("审批动作不能为空", nameof(action));

        if (action != "Approved" && action != "Rejected")
            throw new ArgumentException("审批动作必须为 Approved（通过）或 Rejected（驳回）", nameof(action));

        if (level < 1)
            throw new ArgumentException("审批级别必须大于等于 1", nameof(level));

        var record = new ApprovalRecord(approvalRequestId, level, approverId, action, comment);
        record.SetCreated(createdBy, createdAt);
        return record;
    }

    /// <summary>
    /// 审批是否通过
    /// </summary>
    public bool IsApproved => Action == "Approved";

    /// <summary>
    /// 审批是否驳回
    /// </summary>
    public bool IsRejected => Action == "Rejected";
}
