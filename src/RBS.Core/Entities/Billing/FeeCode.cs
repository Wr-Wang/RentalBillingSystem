namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class FeeCode : AuditableEntity, IHasLandlord
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string BillingMode { get; private set; } = "FixedAmount";
    public string? Unit { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string Category { get; private set; } = "Other";
    public bool IsRequired { get; private set; }
    public Guid LandlordId { get; private set; }
    private FeeCode() { }
    public FeeCode(string code, string name, Guid landlordId) { Code = code; Name = name; LandlordId = landlordId; }
}
