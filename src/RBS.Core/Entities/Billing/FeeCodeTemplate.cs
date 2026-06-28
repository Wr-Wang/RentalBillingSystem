namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class FeeCodeTemplate : AuditableEntity, IHasCompany
{
    public Guid FeeCodeId { get; private set; }
    public string? Description { get; private set; }
    public decimal DefaultAmount { get; private set; }
    public decimal? DefaultUnitPrice { get; private set; }
    public Guid CompanyId { get; private set; }
    private FeeCodeTemplate() { }
    public FeeCodeTemplate(Guid feeCodeId, decimal defaultAmount, Guid companyId)
    { FeeCodeId = feeCodeId; DefaultAmount = defaultAmount; CompanyId = companyId; }
}
