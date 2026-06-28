namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class CollectionStage : AuditableEntity, IHasLandlord
{
    public string Name { get; private set; } = string.Empty;
    public int DaysOverdue { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid LandlordId { get; private set; }
    private CollectionStage() { }
    public CollectionStage(string name, int daysOverdue, Guid landlordId)
    { Name = name; DaysOverdue = daysOverdue; LandlordId = landlordId; }
}
