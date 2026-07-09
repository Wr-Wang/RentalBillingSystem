namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 新建租客请求暂存实体 — 审批通过前暂存新租客数据
/// 审批通过后创建正式 Tenant + ContractTenant
/// </summary>
public class TenantCreateRequest : AuditableEntity, IHasCompany
{
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? IdCard { get; private set; }
    public string? Email { get; private set; }
    public string? Wechat { get; private set; }
    public string? EmergencyContact { get; private set; }
    public string? EmergencyPhone { get; private set; }
    public string? Address { get; private set; }
    public string? Remark { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid ContractId { get; private set; }
    public bool IsPrimary { get; private set; }
    public string Status { get; private set; } = "Draft"; // Draft/PendingApproval/Completed/Rejected
    public Guid? ApprovalRequestId { get; private set; }
    public Guid? NewTenantId { get; private set; }

    private TenantCreateRequest() { }

    public TenantCreateRequest(string name, Guid companyId, Guid contractId, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("姓名不能为空", nameof(name));
        Name = name;
        CompanyId = companyId;
        ContractId = contractId;
        IsPrimary = isPrimary;
        Status = "Draft";
    }

    public void SetContact(string? phone, string? idCard, string? email, string? wechat)
    { Phone = phone; IdCard = idCard; Email = email; Wechat = wechat; }
    public void SetEmergency(string? contact, string? phone) { EmergencyContact = contact; EmergencyPhone = phone; }
    public void SetAddress(string? address) => Address = address;
    public void SetRemark(string? remark) => Remark = remark;
    public void Submit() => Status = "PendingApproval";
    public void Complete(Guid newTenantId) { NewTenantId = newTenantId; Status = "Completed"; }
    public void Reject() => Status = "Rejected";
}
