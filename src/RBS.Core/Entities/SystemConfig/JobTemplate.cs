namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class JobTemplate : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string ShortName { get; private set; } = string.Empty;
    public string DefaultScheduleType { get; private set; } = "Monthly";  // Daily / Monthly
    public int DefaultHour { get; private set; } = 8;
    public int DefaultMinute { get; private set; } = 0;
    public int? DefaultDayOfMonth { get; private set; }
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private JobTemplate() { }

    public JobTemplate(string code, string displayName, string shortName,
        string scheduleType, int hour, int minute, int? dayOfMonth,
        string? description, string? icon, string category, int sortOrder)
    {
        Code = code;
        DisplayName = displayName;
        ShortName = shortName;
        DefaultScheduleType = scheduleType;
        DefaultHour = hour;
        DefaultMinute = minute;
        DefaultDayOfMonth = dayOfMonth;
        Description = description;
        Icon = icon;
        Category = category;
        SortOrder = sortOrder;
    }

    public string DefaultDisplay =>
        DefaultScheduleType == "Daily"
            ? $"每天 {DefaultHour:D2}:{DefaultMinute:D2}"
            : $"每月{DefaultDayOfMonth}日 {DefaultHour:D2}:{DefaultMinute:D2}";
}
