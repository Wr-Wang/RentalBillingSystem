namespace RBS.Application.DTOs.Approval;

public class ApprovalRequestDto
{
    public Guid Id { get; set; }
    public Guid ApprovalTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TargetEntityId { get; set; }
    public string TargetEntityType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; }
    public string? ApprovalTypeName { get; set; }
    public string? SubmitterName { get; set; }
    public string? CurrentLevelName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ApprovalRecordDto> Records { get; set; } = new();
    /// <summary>审批链：每级审批角色及当前状态</summary>
    public List<ApprovalLevelStatusDto> LevelChain { get; set; } = new();
}

public class ApprovalRecordDto
{
    public Guid Id { get; set; }
    public int Level { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    /// <summary>审批人登录账号</summary>
    public string ApproverAccount { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApproveRequest
{
    public string? Comment { get; set; }
}

public class RejectRequest
{
    public string Comment { get; set; } = string.Empty;
}

/// <summary>审批链节点状态</summary>
public class ApprovalLevelStatusDto
{
    public int Level { get; set; }
    public string RoleName { get; set; } = "";
    /// <summary>completed | rejected | current | pending | skipped</summary>
    public string Status { get; set; } = "pending";
    /// <summary>该级审批人姓名（已审批时有值）</summary>
    public string? ApproverName { get; set; }
    /// <summary>该级审批人账号（已审批时有值）</summary>
    public string? ApproverAccount { get; set; }
}

public class CancelRequest
{
    public string? Reason { get; set; }
}

public class LastRejectedApprovalDto
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>审批业务详情 — 统一对比展示</summary>
public class ApprovalBizDetailDto
{
    public string BizType { get; set; } = string.Empty;   // RENT_ADJUST | FEE_ADJUST | TERMINATE
    public string Title { get; set; } = string.Empty;
    public string? EffectiveDate { get; set; }             // 生效日期（独立字段，前端直接展示）
    public List<BizFieldDto> Fields { get; set; } = new();
    public List<BizFeeItemDto>? FeeItems { get; set; }     // 调价时：费用项的逐项对比
}

public class BizFieldDto
{
    public string Label { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public bool IsChanged { get; set; }
}

/// <summary>调价审批的费用项对比明细</summary>
public class BizFeeItemDto
{
    public string FeeName { get; set; } = string.Empty;
    public decimal OldAmount { get; set; }
    public decimal NewAmount { get; set; }
    public string? BillingMode { get; set; }
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
