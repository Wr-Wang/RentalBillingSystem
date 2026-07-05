namespace RBS.Core.Entities.Scheduling;

/// <summary>
/// 任务步骤执行日志 — 记录任务中每个步骤的耗时和影响数
/// 步骤日志与业务数据在同一事务中写入，提交=记录，回滚=不留痕
/// </summary>
public class TaskStepLog
{
    public Guid Id { get; private set; }
    public Guid TaskLogId { get; private set; }
    public string StepName { get; private set; } = string.Empty;
    public string StepDisplayName { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public int SortOrder { get; private set; }
    public string Status { get; private set; } = "Running";
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? DurationMs { get; private set; }
    public int? AffectedCount { get; private set; }
    public string? Message { get; private set; }
    public string? ErrorMessage { get; private set; }

    private TaskStepLog() { }

    public TaskStepLog(Guid taskLogId, string stepName, string displayName, Guid? parentId = null, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        TaskLogId = taskLogId;
        StepName = stepName;
        StepDisplayName = displayName;
        ParentId = parentId;
        SortOrder = sortOrder;
        Status = "Running";
        StartedAt = RBS.Core.Common.ChinaTime.Now;
    }

    public void Complete(int affectedCount)
    {
        Status = "Completed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        DurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
        AffectedCount = affectedCount;
    }

    public void Fail(string error)
    {
        Status = "Failed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        DurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
        ErrorMessage = error;
    }

    public void Skip(string reason)
    {
        Status = "Skipped";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        Message = reason;
    }
}
