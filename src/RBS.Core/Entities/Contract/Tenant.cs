namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 租客实体 — 独立的聚合根（非 Contract 子实体）
/// 管理租客基本信息，租客可与多个合同关联（通过 ContractTenant 中间表）
/// </summary>
public class Tenant : AuditableEntity, IHasCompany
{
    // ===== 基础信息 =====
    /// <summary>租客姓名，业务必填字段</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>身份证号，可为 null 表示未录入</summary>
    public string? IdCard { get; private set; }
    /// <summary>联系电话，用于催缴和日常联系</summary>
    public string? Phone { get; private set; }
    /// <summary>电子邮箱</summary>
    public string? Email { get; private set; }
    /// <summary>所属公司标识，用于多租户数据隔离</summary>
    public Guid CompanyId { get; private set; }
    /// <summary>是否启用，false 表示已注销/黑名单</summary>
    public bool IsActive { get; private set; } = true;

    // ===== 扩充字段 =====
    /// <summary>微信号码</summary>
    public string? Wechat { get; private set; }
    /// <summary>紧急联系人姓名</summary>
    public string? EmergencyContact { get; private set; }
    /// <summary>紧急联系人电话</summary>
    public string? EmergencyPhone { get; private set; }
    /// <summary>联系地址</summary>
    public string? Address { get; private set; }
    /// <summary>备注信息</summary>
    public string? Remark { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private Tenant() { }

    /// <summary>
    /// 创建租客实例
    /// </summary>
    /// <param name="name">租客姓名，不能为空</param>
    /// <param name="companyId">所属公司标识</param>
    /// <exception cref="ArgumentException">当姓名为空或空白时抛出</exception>
    public Tenant(string name, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("租客姓名不能为空", nameof(name));
        Name = name;
        CompanyId = companyId;
    }

    // ===== 已有 Setter =====
    /// <summary>
    /// 修改租客姓名
    /// </summary>
    /// <param name="name">新姓名，不能为空</param>
    /// <exception cref="ArgumentException">当姓名为空或空白时抛出</exception>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("租客姓名不能为空", nameof(name));
        Name = name;
    }
    /// <summary>设置联系电话，null 表示清空</summary>
    public void SetPhone(string? phone) => Phone = phone;
    /// <summary>设置身份证号，null 表示清空</summary>
    public void SetIdCard(string? idCard) => IdCard = idCard;
    /// <summary>设置电子邮箱，null 表示清空</summary>
    public void SetEmail(string? email) => Email = email;
    /// <summary>启用租客（设为活跃状态）</summary>
    public void Activate() => IsActive = true;
    /// <summary>停用租客（设为非活跃，相当于注销）</summary>
    public void Deactivate() => IsActive = false;

    // ===== 新增 Setter =====
    /// <summary>设置微信号码，null 表示清空</summary>
    public void SetWechat(string? wechat) => Wechat = wechat;
    /// <summary>设置紧急联系人信息</summary>
    /// <param name="contact">联系人姓名，null 表示清空</param>
    /// <param name="phone">联系人电话，null 表示清空</param>
    public void SetEmergency(string? contact, string? phone) { EmergencyContact = contact; EmergencyPhone = phone; }
    /// <summary>设置联系地址，null 表示清空</summary>
    public void SetAddress(string? address) => Address = address;
    /// <summary>设置备注信息，null 表示清空</summary>
    public void SetRemark(string? remark) => Remark = remark;
}
