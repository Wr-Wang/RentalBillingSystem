namespace RBS.Application.DTOs.SystemConfig;

public class JobTemplateDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string DefaultScheduleType { get; set; } = "Monthly";
    public int DefaultHour { get; set; }
    public int DefaultMinute { get; set; }
    public int? DefaultDayOfMonth { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class JobScheduleDto
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = "Monthly";
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int? DayOfMonth { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string? TemplateCode { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }
}

public class CreateJobScheduleRequest
{
    public string JobName { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = "Monthly";
    public int Hour { get; set; } = 8;
    public int Minute { get; set; } = 0;
    public int? DayOfMonth { get; set; }
    public string? Description { get; set; }
    public string? TemplateCode { get; set; }
}

public class UpdateJobScheduleRequest
{
    public string? JobName { get; set; }
    public string? ScheduleType { get; set; }
    public int? Hour { get; set; }
    public int? Minute { get; set; }
    public int? DayOfMonth { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class ExecutionDto
{
    public Guid Id { get; set; }
    public Guid JobScheduleId { get; set; }
    public string Month { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public DateTime? OriginalDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Reason { get; set; }
    public bool IsAdjusted { get; set; }
    public bool IsCustom { get; set; }
}

public class CreateExecutionRequest
{
    public DateTime TargetDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Reason { get; set; }
}

public class UpdateExecutionRequest
{
    public DateTime? TargetDate { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
}
