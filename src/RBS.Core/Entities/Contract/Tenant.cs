namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>租客实体</summary>
public class Tenant : AuditableEntity, IHasCompany
{
    public string Name { get; private set; } = string.Empty;
    public string? IdCard { get; private set; }
    public string? Phone { get; private set; }
    public Guid CompanyId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public Tenant(string name, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("租客姓名不能为空", nameof(name));
        Name = name;
        CompanyId = companyId;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("租客姓名不能为空", nameof(name));
        Name = name;
    }

    public void SetPhone(string? phone) => Phone = phone;
    public void SetIdCard(string? idCard) => IdCard = idCard;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
