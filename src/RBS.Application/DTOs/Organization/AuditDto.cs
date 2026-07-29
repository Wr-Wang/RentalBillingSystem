namespace RBS.Application.DTOs.Organization;

/// <summary>
/// 审计日志条目（v2 — 支持关键字段 + 变更字段分离展示）
/// </summary>
public class AuditEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string AuditAction { get; set; } = string.Empty;
    public int AuditVersionNo { get; set; }
    public DateTime AuditChangedAt { get; set; }
    public Guid AuditChangedBy { get; set; }

    /// <summary>实体中文展示名（如"合同"、"公司"），由配置提供</summary>
    public string EntityDisplayName { get; set; } = string.Empty;

    /// <summary>关键标识字段（用于识别是哪条记录），由配置的 keyFields 决定</summary>
    public Dictionary<string, object?> KeyValues { get; set; } = new();

    /// <summary>发生变更的字段名列表（仅 Update 有值；Insert/Delete 为空）</summary>
    public List<string> ChangedFieldNames { get; set; } = new();

    /// <summary>变更字段的新值（仅 Update 有值）</summary>
    public Dictionary<string, object?> ChangedValues { get; set; } = new();

    /// <summary>操作人姓名（由前端通过 AuditChangedBy 查询后填充）</summary>
    public string? ChangedByName { get; set; }
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
/// 审计回滚结果
/// </summary>
public class AuditRollbackResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Table { get; set; }
    public string? RecordId { get; set; }
    public int VersionNo { get; set; }
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
