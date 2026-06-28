namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class MeterEstimationConfig : AuditableEntity, IHasLandlord
{
    public Guid FeeCodeId { get; private set; }
    public decimal EstimatedUsage { get; private set; }
    public string? Remark { get; private set; }
    public Guid LandlordId { get; private set; }
    private MeterEstimationConfig() { }
    public MeterEstimationConfig(Guid feeCodeId, decimal estimatedUsage, Guid landlordId)
    { FeeCodeId = feeCodeId; EstimatedUsage = estimatedUsage; LandlordId = landlordId; }
}
