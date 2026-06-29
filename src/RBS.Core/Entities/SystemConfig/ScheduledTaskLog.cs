namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class ScheduledTaskLog : AuditableEntity, IHasCompany
{
    public string TaskName { get; private set; } = string.Empty;
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? ErrorMessage { get; private set; }
    public Guid CompanyId { get; private set; }
    public string? TargetMonth { get; private set; }      // yyyy-MM，执行锁核心字段
    public DateTime? HeartbeatAt { get; private set; }     // 心跳，检测僵死任务

    private ScheduledTaskLog() { }
    public ScheduledTaskLog(string taskName, Guid companyId) { TaskName = taskName; CompanyId = companyId; }

    public void SetRunning(string targetMonth)
    {
        Status = "Running";
        TargetMonth = targetMonth;
        StartedAt = DateTime.UtcNow;
        HeartbeatAt = DateTime.UtcNow;
    }

    public void Heartbeat() => HeartbeatAt = DateTime.UtcNow;

    public void Complete()
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = "Failed";
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkStale()
    {
        if (Status == "Running") Status = "Stale";
    }
}
