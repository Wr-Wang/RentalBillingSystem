namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>租客实体</summary>
public class Tenant : AuditableEntity, IHasCompany
{
    // ===== 基础信息 =====
    public string Name { get; private set; } = string.Empty;
    public string? IdCard { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public Guid CompanyId { get; private set; }
    public bool IsActive { get; private set; } = true;

    // ===== 扩充字段 =====
    public string? Wechat { get; private set; }
    public string? EmergencyContact { get; private set; }
    public string? EmergencyPhone { get; private set; }
    public string? Address { get; private set; }
    public string? Remark { get; private set; }

    private Tenant() { }

    public Tenant(string name, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("租客姓名不能为空", nameof(name));
        Name = name;
        CompanyId = companyId;
    }

    // ===== 已有 Setter =====
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("租客姓名不能为空", nameof(name));
        Name = name;
    }
    public void SetPhone(string? phone) => Phone = phone;
    public void SetIdCard(string? idCard) => IdCard = idCard;
    public void SetEmail(string? email) => Email = email;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    // ===== 新增 Setter =====
    public void SetWechat(string? wechat) => Wechat = wechat;
    public void SetEmergency(string? contact, string? phone) { EmergencyContact = contact; EmergencyPhone = phone; }
    public void SetAddress(string? address) => Address = address;
    public void SetRemark(string? remark) => Remark = remark;
}
