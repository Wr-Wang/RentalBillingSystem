namespace RBS.Application.DTOs.Approval;

/// <summary>
/// 审批历史查询参数
/// </summary>
public class ApprovalHistoryQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}
