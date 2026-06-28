namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

public class Tenant : AuditableEntity, IHasLandlord
{
    public string Name { get; private set; } = string.Empty;
    public string? IdCard { get; private set; }
    public string? Phone { get; private set; }
    public Guid LandlordId { get; private set; }
    public bool IsActive { get; private set; } = true;
    private Tenant() { }
    public Tenant(string name, Guid landlordId) { Name = name; LandlordId = landlordId; }
}
