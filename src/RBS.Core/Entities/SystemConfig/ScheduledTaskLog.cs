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
    private ScheduledTaskLog() { }
    public ScheduledTaskLog(string taskName, Guid companyId) { TaskName = taskName; CompanyId = companyId; }
}
