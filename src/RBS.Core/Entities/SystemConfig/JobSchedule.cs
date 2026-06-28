namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class JobSchedule : AuditableEntity, IHasLandlord
{
    public string JobName { get; private set; } = string.Empty;
    public string CronExpression { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }
    public Guid LandlordId { get; private set; }
    private JobSchedule() { }
    public JobSchedule(string jobName, string cronExpression, Guid landlordId)
    { JobName = jobName; CronExpression = cronExpression; LandlordId = landlordId; }
}
