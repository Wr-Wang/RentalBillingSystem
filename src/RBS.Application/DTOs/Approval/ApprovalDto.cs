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
}

public class ApprovalRecordDto
{
    public Guid Id { get; set; }
    public int Level { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
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
