namespace RBS.Application.DTOs.SystemConfig;

public class JobScheduleDto
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

public class CreateJobScheduleRequest
{
    public string JobName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateJobScheduleRequest
{
    public string? CronExpression { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
