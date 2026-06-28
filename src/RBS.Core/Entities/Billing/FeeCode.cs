namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class FeeCode : AuditableEntity, IHasCompany
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string BillingMode { get; private set; } = "FixedAmount";
    public string? Unit { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string Category { get; private set; } = "Other";
    public bool IsRequired { get; private set; }
    public Guid CompanyId { get; private set; }
    private FeeCode() { }
    public FeeCode(string code, string name, Guid companyId) { Code = code; Name = name; CompanyId = companyId; }

    public void Rename(string name) => Name = name;
    public void SetCode(string code) => Code = code;
    public void SetBillingMode(string mode) => BillingMode = mode;
    public void SetUnit(string? unit) => Unit = unit;
    public void SetSortOrder(int order) => SortOrder = order;
    public void SetCategory(string category) => Category = category;
    public void SetRequired(bool required) => IsRequired = required;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
