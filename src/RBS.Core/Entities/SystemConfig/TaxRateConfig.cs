namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class TaxRateConfig : AuditableEntity, IHasCompany
{
    public string Name { get; private set; } = string.Empty;
    public decimal Rate { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }
    private TaxRateConfig() { }
    public TaxRateConfig(string name, decimal rate, DateOnly effectiveDate, Guid companyId)
    { Name = name; Rate = rate; EffectiveDate = effectiveDate; CompanyId = companyId; }
    public void Rename(string name) => Name = name;
    public void SetRate(decimal rate) => Rate = rate;
    public void SetEffectiveDate(DateOnly date) => EffectiveDate = date;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
