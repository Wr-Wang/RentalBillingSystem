namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 排期执行实例 — 每条记录一个具体的执行日期时间
/// </summary>
public class JobScheduleExecution : AuditableEntity, IHasCompany
{
    public Guid JobScheduleId { get; private set; }
    // public JobSchedule JobSchedule { get; private set; } = null!; // Dapper不支持导航属性
    public Guid CompanyId { get; private set; }

    public DateTime TargetDate { get; private set; }
    public DateTime? OriginalDate { get; private set; }
    public string Month { get; private set; } = null!;
    public string Status { get; private set; } = "Pending";
    public string? Reason { get; private set; }
    public bool IsAdjusted { get; private set; }
    public bool IsCustom { get; private set; }

    private JobScheduleExecution() { }

    public JobScheduleExecution(Guid jobScheduleId, Guid companyId,
        DateTime targetDate, DateTime? originalDate, string month, bool isCustom)
    {
        JobScheduleId = jobScheduleId;
        CompanyId = companyId;
        TargetDate = targetDate;
        OriginalDate = originalDate;
        Month = month;
        IsCustom = isCustom;
        Status = "Pending";
    }

    public void Update(DateTime targetDate, string status, string? reason)
    {
        TargetDate = targetDate;
        Status = status;
        Reason = reason;
        IsAdjusted = true;
    }

    public void MarkAdjusted() => IsAdjusted = true;

    public void MarkProcessing() => Status = "Processing";
    public void MarkCompleted() { Status = "Completed"; IsAdjusted = true; }
    public void MarkFailed(string? reason) { Status = "Failed"; Reason = reason; }

    /// <summary>跳过（上游失败阻断或管理员手动跳过）</summary>
    public void MarkSkipped(string? reason) { Status = "Skipped"; Reason = reason; }

    /// <summary>暂停（管理员手动暂停，调度引擎不会拾取）</summary>
    public void MarkPaused(string? reason) { Status = "Paused"; Reason = reason; }

    /// <summary>取消（管理员手动取消，终态不可逆转）</summary>
    public void MarkCancelled(string? reason) { Status = "Cancelled"; Reason = reason; }

    /// <summary>重置为待执行（从 Skipped/Paused/Failed 恢复）</summary>
    public void ResetToPending(string? reason = null) { Status = "Pending"; Reason = reason; }
}
