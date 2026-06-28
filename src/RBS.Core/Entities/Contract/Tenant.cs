namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

public class Tenant : AuditableEntity, IHasCompany
{
    public string Name { get; private set; } = string.Empty;
    public string? IdCard { get; private set; }
    public string? Phone { get; private set; }
    public Guid CompanyId { get; private set; }
    public bool IsActive { get; private set; } = true;
    private Tenant() { }
    public Tenant(string name, Guid companyId) { Name = name; CompanyId = companyId; }
}
