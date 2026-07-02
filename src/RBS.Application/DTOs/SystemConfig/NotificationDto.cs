namespace RBS.Application.DTOs.SystemConfig;

/// <summary>通知列表项</summary>
public class NotificationDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>通知列表查询参数</summary>
public class NotificationQueryDto
{
    public string? Category { get; set; }
    public bool? IsRead { get; set; }
    public string? Keyword { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>各分类未读计数</summary>
public class UnreadCountsDto
{
    public int Approval { get; set; }
    public int Renewal { get; set; }
    public int Collection { get; set; }
    public int System { get; set; }
    public int Total { get; set; }
}
