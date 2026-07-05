namespace RBS.Core.Entities.Scheduling;

using RBS.Core.Entities.Base;

/// <summary>
/// 任务执行日志 — 记录每次任务执行的完整信息
/// 替换旧 ScheduledTaskLogs
/// </summary>
public class TaskLog : AuditableEntity
{
    public string TaskName { get; private set; } = string.Empty;
    public Guid CompanyId { get; private set; }
    public Guid? ContractId { get; private set; }
    public string TargetMonth { get; private set; } = string.Empty;
    public string TriggerType { get; private set; } = "Scheduled";
    public string RunMode { get; private set; } = "Execute";
    public string Status { get; private set; } = "Running";
    public string? Params { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? TotalDurationMs { get; private set; }
    public int? TotalCount { get; private set; }
    public int? SuccessCount { get; private set; }
    public int? FailCount { get; private set; }
    public int? WarningCount { get; private set; }
    public string? Summary { get; private set; }
    public DateTime? HeartbeatAt { get; private set; }
    public string? ResultData { get; private set; }
    public string? ErrorMessage { get; private set; }

    private TaskLog() { }

    public TaskLog(string taskName, Guid companyId, string targetMonth,
        string triggerType = "Scheduled", string runMode = "Execute")
    {
        TaskName = taskName;
        CompanyId = companyId;
        TargetMonth = targetMonth;
        TriggerType = triggerType;
        RunMode = runMode;
        Status = "Running";
        StartedAt = RBS.Core.Common.ChinaTime.Now;
    }

    public void Complete(int totalCount, int successCount, int failCount, int warningCount, string? summary)
    {
        Status = "Completed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        TotalCount = totalCount;
        SuccessCount = successCount;
        FailCount = failCount;
        WarningCount = warningCount;
        Summary = summary;
        TotalDurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
    }

    public void Fail(string error)
    {
        Status = "Failed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        ErrorMessage = error;
        TotalDurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
    }

    public void MarkStale()
    {
        Status = "Stale";
    }

    public void UpdateHeartbeat()
    {
        HeartbeatAt = RBS.Core.Common.ChinaTime.Now;
    }

    public void SetDryRunResult(string resultData)
    {
        ResultData = resultData;
        Status = "Completed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
    }
}
