namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class MeterEstimationConfig : AuditableEntity, IHasCompany
{
    public Guid FeeCodeId { get; private set; }
    public decimal EstimatedUsage { get; private set; }
    public string? Remark { get; private set; }
    public Guid CompanyId { get; private set; }
    private MeterEstimationConfig() { }
    public MeterEstimationConfig(Guid feeCodeId, decimal estimatedUsage, Guid companyId)
    { FeeCodeId = feeCodeId; EstimatedUsage = estimatedUsage; CompanyId = companyId; }
}
