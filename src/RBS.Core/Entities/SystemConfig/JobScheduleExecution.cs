namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 排期执行实例 — 每条记录一个具体的执行日期时间
/// </summary>
public class JobScheduleExecution : AuditableEntity, IHasCompany
{
    public Guid JobScheduleId { get; private set; }
    public JobSchedule JobSchedule { get; private set; } = null!;
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
}
