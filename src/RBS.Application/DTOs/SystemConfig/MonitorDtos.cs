namespace RBS.Application.DTOs.SystemConfig;

// ===== Dashboard =====

public class DashboardStatsDto
{
    public int TodayTotal { get; set; }
    public int TodaySuccess { get; set; }
    public int TodayFailed { get; set; }
    public int RunningCount { get; set; }
    public int YesterdayTotal { get; set; }
    public double SuccessRate => TodayTotal > 0
        ? Math.Round((double)TodaySuccess / TodayTotal * 100, 1) : 100;
    public int DiffYesterday => TodayTotal - YesterdayTotal;
}

public class TrendPointDto
{
    public string Date { get; set; } = null!;
    public double SuccessRate { get; set; }
    public int TotalCount { get; set; }
    public int FailCount { get; set; }
}

public class TaskAvgDurationDto
{
    public string TaskName { get; set; } = null!;
    public double AvgDurationMs { get; set; }
    public int ExecutionCount { get; set; }
}

public class FailureAggregationDto
{
    public string ErrorCategory { get; set; } = null!;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public string Trend { get; set; } = "→";
}

// ===== 日志查询 =====

public class TaskLogQuery
{
    public string? TaskName { get; set; }
    public string? Status { get; set; }
    public string? TriggerType { get; set; }
    public string? RunMode { get; set; }
    public string? Keyword { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Guid? CompanyId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TaskLogListItemDto
{
    public Guid Id { get; set; }
    public string TaskName { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string TargetMonth { get; set; } = null!;
    public string TriggerType { get; set; } = null!;
    public string RunMode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? TotalDurationMs { get; set; }
    public int? TotalCount { get; set; }
    public int? SuccessCount { get; set; }
    public int? FailCount { get; set; }
    public int? WarningCount { get; set; }
    public string? Summary { get; set; }
    public string? ErrorMessage { get; set; }
}

// ===== 步骤详情 =====

public class TaskLogDetailDto
{
    public Guid Id { get; set; }
    public string TaskName { get; set; } = null!;
    public string TargetMonth { get; set; } = null!;
    public string TriggerType { get; set; } = null!;
    public string RunMode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? TotalDurationMs { get; set; }
    public int? TotalCount { get; set; }
    public int? SuccessCount { get; set; }
    public int? FailCount { get; set; }
    public int? WarningCount { get; set; }
    public string? Summary { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CompanyName { get; set; }
    public List<StepDetailDto> Steps { get; set; } = new();
}

public class StepDetailDto
{
    public Guid Id { get; set; }
    public string StepDisplayName { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public string Status { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationMs { get; set; }
    public int? AffectedCount { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}

// ===== 反转预览 =====

public class ReversePreviewDto
{
    public Guid TaskLogId { get; set; }
    public string TaskName { get; set; } = null!;
    public string TargetMonth { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public bool HasPayment { get; set; }
    public int DebitNoteCount { get; set; }
    public int JournalCount { get; set; }
}

// ===== 失败合同 =====

public class FailedContractDto
{
    public long Id { get; set; }
    public Guid TaskLogId { get; set; }
    public Guid ContractId { get; set; }
    public string ContractNo { get; set; } = "";
    public string StepName { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public DateTime FailedAt { get; set; }
    public bool IsRetried { get; set; }
    public DateTime? RetriedAt { get; set; }

    /// <summary>步骤中文名（前端展示用）</summary>
    public string StepDisplayName => StepName switch
    {
        "BillStep02" => "加载费用配置",
        "BillStep03" => "生成应收/GL",
        "BillStep04" => "生成账单快照",
        "SettleStep01" => "查询待处理合同",
        "SettleStep02" => "预收抵应收",
        "SettleStep03" => "计算逾期利息",
        _ => StepName
    };
}

// 分页结果复用 RBS.Core.Interfaces.Repositories.PagedResult<T>
