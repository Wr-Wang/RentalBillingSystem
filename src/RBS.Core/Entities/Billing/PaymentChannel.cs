namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class PaymentChannel : AuditableEntity, IHasCompany
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }
    private PaymentChannel() { }
    public PaymentChannel(string name, string code, Guid companyId) { Name = name; Code = code; CompanyId = companyId; }
    public void Rename(string name) => Name = name;
    public void SetCode(string code) => Code = code;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
