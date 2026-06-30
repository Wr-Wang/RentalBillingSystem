namespace RBS.Application.DTOs.Approval;

/// <summary>
/// 提交审批请求
/// </summary>
public class SubmitApprovalRequest
{
    public Guid ApprovalTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TargetEntityId { get; set; }
    public string TargetEntityType { get; set; } = string.Empty;
}
