namespace RBS.Application.DTOs.Organization;

/// <summary>
/// 审计日志条目
/// </summary>
public class AuditEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string AuditAction { get; set; } = string.Empty;
    public int AuditVersionNo { get; set; }
    public DateTime AuditChangedAt { get; set; }
    public Guid AuditChangedBy { get; set; }
    public Dictionary<string, object?> Changes { get; set; } = new();
}

/// <summary>
/// 审计统计
/// </summary>
public class AuditStatsDto
{
    public int TodayCount { get; set; }
    public int WeekCount { get; set; }
    public int MonthCount { get; set; }
    public int TotalTables { get; set; }
}

/// <summary>
/// 审计日志分页查询参数
/// </summary>
public class AuditQuery
{
    public string TableName { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// 审计版本对比结果
/// </summary>
public class AuditCompareDto
{
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public bool Changed { get; set; }
}
