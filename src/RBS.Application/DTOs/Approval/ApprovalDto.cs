namespace RBS.Application.DTOs.Approval;

/// <summary>
/// 审批请求数据传输对象 — 审批列表及详情展示
/// </summary>
public class ApprovalRequestDto
{
    /// <summary>审批请求 ID</summary>
    public Guid Id { get; set; }
    /// <summary>审批类型 ID</summary>
    public Guid ApprovalTypeId { get; set; }
    /// <summary>审批标题</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>审批描述</summary>
    public string? Description { get; set; }
    /// <summary>目标业务实体 ID</summary>
    public Guid TargetEntityId { get; set; }
    /// <summary>目标业务实体类型：Import / ContractFeeAdjust / ContractTerminate / ContractRenewal / ContractActivation / ...</summary>
    public string TargetEntityType { get; set; } = string.Empty;
    /// <summary>审批状态：Pending / Approved / Rejected / Cancelled</summary>
    public string Status { get; set; } = "Pending";
    /// <summary>当前审批级别</summary>
    public int CurrentLevel { get; set; }
    /// <summary>最大审批级别</summary>
    public int MaxLevel { get; set; }
    /// <summary>审批类型名称</summary>
    public string? ApprovalTypeName { get; set; }
    /// <summary>提交人姓名</summary>
    public string? SubmitterName { get; set; }
    /// <summary>当前审批级别名称（如"经理审批"）</summary>
    public string? CurrentLevelName { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>完成时间</summary>
    public DateTime? CompletedAt { get; set; }
    /// <summary>审批记录列表</summary>
    public List<ApprovalRecordDto> Records { get; set; } = new();
    /// <summary>审批链：每级审批角色及当前状态</summary>
    public List<ApprovalLevelStatusDto> LevelChain { get; set; } = new();
}

/// <summary>
/// 审批操作记录数据传输对象
/// </summary>
public class ApprovalRecordDto
{
    /// <summary>记录 ID</summary>
    public Guid Id { get; set; }
    /// <summary>审批级别</summary>
    public int Level { get; set; }
    /// <summary>审批人 ID</summary>
    public Guid ApproverId { get; set; }
    /// <summary>审批人姓名</summary>
    public string ApproverName { get; set; } = string.Empty;
    /// <summary>审批人登录账号</summary>
    public string ApproverAccount { get; set; } = string.Empty;
    /// <summary>操作：Submitted / Approved / Rejected / Cancelled</summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>审批意见</summary>
    public string? Comment { get; set; }
    /// <summary>操作时间</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 审批通过请求
/// </summary>
public class ApproveRequest
{
    /// <summary>审批意见</summary>
    public string? Comment { get; set; }
}

/// <summary>
/// 审批驳回请求
/// </summary>
public class RejectRequest
{
    /// <summary>驳回原因（必填）</summary>
    public string Comment { get; set; } = string.Empty;
}

/// <summary>审批链节点状态</summary>
public class ApprovalLevelStatusDto
{
    /// <summary>审批级别（0 表示提交）</summary>
    public int Level { get; set; }
    /// <summary>角色名称</summary>
    public string RoleName { get; set; } = "";
    /// <summary>状态：completed | rejected | current | pending | skipped</summary>
    public string Status { get; set; } = "pending";
    /// <summary>该级审批人姓名（已审批时有值）</summary>
    public string? ApproverName { get; set; }
    /// <summary>该级审批人账号（已审批时有值）</summary>
    public string? ApproverAccount { get; set; }
}

/// <summary>
/// 撤回审批请求
/// </summary>
public class CancelRequest
{
    /// <summary>撤回原因</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// 最近一次被驳回的审批数据（用于重新提交预填）
/// </summary>
public class LastRejectedApprovalDto
{
    /// <summary>审批描述（含驳回时的上下文）</summary>
    public string? Description { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>审批业务详情 — 统一对比展示</summary>
public class ApprovalBizDetailDto
{
    /// <summary>业务类型：RENT_ADJUST | FEE_ADJUST | TERMINATE | ContractRenewal | ContractActivation</summary>
    public string BizType { get; set; } = string.Empty;
    /// <summary>审批标题</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>生效日期（独立字段，前端直接展示）</summary>
    public string? EffectiveDate { get; set; }
    /// <summary>比较字段列表</summary>
    public List<BizFieldDto> Fields { get; set; } = new();
    /// <summary>调价时：费用项的逐项对比</summary>
    public List<BizFeeItemDto>? FeeItems { get; set; }
}

/// <summary>
/// 审批对比字段数据传输对象
/// </summary>
public class BizFieldDto
{
    /// <summary>字段标签</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>旧值（审批前）</summary>
    public string? OldValue { get; set; }
    /// <summary>新值（审批后）</summary>
    public string? NewValue { get; set; }
    /// <summary>是否发生变化</summary>
    public bool IsChanged { get; set; }
}

/// <summary>调价审批的费用项对比明细</summary>
public class BizFeeItemDto
{
    /// <summary>费用名称</summary>
    public string FeeName { get; set; } = string.Empty;
    /// <summary>原金额</summary>
    public decimal OldAmount { get; set; }
    /// <summary>新金额</summary>
    public decimal NewAmount { get; set; }
    /// <summary>计费方式</summary>
    public string? BillingMode { get; set; }
    /// <summary>计量单位</summary>
    public string? Unit { get; set; }
    /// <summary>生效日期（单项独立，null 时取全局生效日）</summary>
    public string? EffectiveDate { get; set; }
    /// <summary>旧配置生效日期（查当前活跃 ContractFeeConfig）</summary>
    public string? OldEffectiveDate { get; set; }
    /// <summary>旧配置到期日期（null = 至今有效）</summary>
    public string? OldExpiryDate { get; set; }
    /// <summary>旧计费方式</summary>
    public string? OldBillingMode { get; set; }
    /// <summary>旧计量单位</summary>
    public string? OldUnit { get; set; }
    /// <summary>收费类型：Recurring / OneTime</summary>
    public string? ChargeType { get; set; }
}
