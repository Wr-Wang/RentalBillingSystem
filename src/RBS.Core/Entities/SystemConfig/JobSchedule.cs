namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

public class JobSchedule : AuditableEntity, IHasCompany
{
    public string JobName { get; private set; } = string.Empty;
    public string ScheduleType { get; private set; } = "Monthly";  // Daily / Monthly
    public int Hour { get; private set; } = 8;
    public int Minute { get; private set; } = 0;
    public int? DayOfMonth { get; private set; }  // 仅 Monthly 类型
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }
    public Guid CompanyId { get; private set; }

    public string? TemplateCode { get; private set; }
    public DateTime? LastRunAt { get; private set; }
    public string? LastRunStatus { get; private set; }

    private JobSchedule() { }
    public JobSchedule(string jobName, string scheduleType, int hour, int minute, Guid companyId, int? dayOfMonth = null)
    {
        JobName = jobName;
        ScheduleType = scheduleType;
        Hour = hour;
        Minute = minute;
        DayOfMonth = (scheduleType == "Monthly") ? (dayOfMonth ?? 1) : null;
        CompanyId = companyId;
    }

    public void SetJobName(string name) => JobName = name;
    public void SetSchedule(string type, int hour, int minute, int? dayOfMonth = null)
    {
        ScheduleType = type;
        Hour = hour;
        Minute = minute;
        DayOfMonth = (type == "Monthly") ? (dayOfMonth ?? 1) : null;
    }
    public void SetDescription(string? desc) => Description = desc;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void SetTemplateCode(string? code) => TemplateCode = code;
    public void RecordRun(string status)
    {
        LastRunAt = ChinaTime.Now;
        LastRunStatus = status;
    }

    /// <summary>获取调度描述，如"每月25日 08:00"</summary>
    public string ScheduleDisplay =>
        ScheduleType == "Daily"
            ? $"每天 {Hour:D2}:{Minute:D2}"
            : $"每月{DayOfMonth}日 {Hour:D2}:{Minute:D2}";
}
