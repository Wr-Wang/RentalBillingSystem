namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class FeeCodeTemplate : AuditableEntity, IHasLandlord
{
    public Guid FeeCodeId { get; private set; }
    public string? Description { get; private set; }
    public decimal DefaultAmount { get; private set; }
    public decimal? DefaultUnitPrice { get; private set; }
    public Guid LandlordId { get; private set; }
    private FeeCodeTemplate() { }
    public FeeCodeTemplate(Guid feeCodeId, decimal defaultAmount, Guid landlordId)
    { FeeCodeId = feeCodeId; DefaultAmount = defaultAmount; LandlordId = landlordId; }
}
