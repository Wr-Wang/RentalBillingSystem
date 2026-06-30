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
    /// <summary>completed | current | pending | skipped</summary>
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
