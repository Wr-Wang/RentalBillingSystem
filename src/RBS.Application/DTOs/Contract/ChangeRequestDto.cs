namespace RBS.Application.DTOs.Contract;

/// <summary>
/// 合同变更请求 DTO
/// </summary>
public class ChangeRequestDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid CompanyId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? StatusLabel { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public string? Reason { get; set; }
    public Guid? ApprovalRequestId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ChangeRequestItemDto> Items { get; set; } = new();
}

/// <summary>
/// 创建变更请求的输入
/// </summary>
public class CreateChangeRequestDto
{
    public Guid ContractId { get; set; }
    public Guid CompanyId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public List<ChangeRequestItemDto> Items { get; set; } = new();
}

/// <summary>
/// 变更请求项 DTO
/// </summary>
public class ChangeRequestItemDto
{
    public string TargetType { get; set; } = string.Empty;
    public Guid? TargetId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string NewValue { get; set; } = string.Empty;
    public decimal? OldValueDecimal { get; set; }
    public decimal? NewValueDecimal { get; set; }
}
