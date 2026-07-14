namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 新建租客请求暂存实体 — 审批通过前暂存新租客数据
/// 审批通过后创建正式 Tenant + ContractTenant 记录
/// 用于在合同创建过程中同步录入新租客的场景
/// </summary>
public class TenantCreateRequest : AuditableEntity, IHasCompany
{
    /// <summary>租客姓名，业务必填字段</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>联系电话</summary>
    public string? Phone { get; private set; }
    /// <summary>身份证号</summary>
    public string? IdCard { get; private set; }
    /// <summary>电子邮箱</summary>
    public string? Email { get; private set; }
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
    /// <summary>所属公司标识</summary>
    public Guid CompanyId { get; private set; }
    /// <summary>目标合同标识，审批通过后创建 ContractTenant 关联到此合同</summary>
    public Guid ContractId { get; private set; }
    /// <summary>是否为主承租人，审批通过后传递给 ContractTenant</summary>
    public bool IsPrimary { get; private set; }
    /// <summary>请求状态：Draft（草稿）/ PendingApproval（待审批）/ Completed（已完成）/ Rejected（已驳回）</summary>
    public string Status { get; private set; } = "Draft";
    /// <summary>关联的审批请求标识</summary>
    public Guid? ApprovalRequestId { get; private set; }
    /// <summary>审批通过后创建的新租客标识（Tenant.Id），null 表示尚未完成</summary>
    public Guid? NewTenantId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private TenantCreateRequest() { }

    /// <summary>
    /// 创建租客新增请求
    /// </summary>
    /// <param name="name">租客姓名，不能为空</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="contractId">关联合同标识</param>
    /// <param name="isPrimary">是否为主承租人，默认 false</param>
    /// <exception cref="ArgumentException">当姓名为空或空白时抛出</exception>
    public TenantCreateRequest(string name, Guid companyId, Guid contractId, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("姓名不能为空", nameof(name));
        Name = name;
        CompanyId = companyId;
        ContractId = contractId;
        IsPrimary = isPrimary;
        Status = "Draft";
    }

    /// <summary>批量设置联系方式</summary>
    /// <param name="phone">电话，null 表示不修改</param>
    /// <param name="idCard">身份证号，null 表示不修改</param>
    /// <param name="email">邮箱，null 表示不修改</param>
    /// <param name="wechat">微信，null 表示不修改</param>
    public void SetContact(string? phone, string? idCard, string? email, string? wechat)
    { Phone = phone; IdCard = idCard; Email = email; Wechat = wechat; }
    /// <summary>设置紧急联系人信息</summary>
    public void SetEmergency(string? contact, string? phone) { EmergencyContact = contact; EmergencyPhone = phone; }
    /// <summary>设置地址</summary>
    public void SetAddress(string? address) => Address = address;
    /// <summary>设置备注</summary>
    public void SetRemark(string? remark) => Remark = remark;
    /// <summary>提交审批，状态变更为 PendingApproval</summary>
    public void Submit() => Status = "PendingApproval";
    /// <summary>审批通过完成创建，记录新租客标识并置状态为 Completed</summary>
    /// <param name="newTenantId">审批通过后创建的正式 Tenant 标识</param>
    public void Complete(Guid newTenantId) { NewTenantId = newTenantId; Status = "Completed"; }
    /// <summary>驳回请求，状态变更为 Rejected</summary>
    public void Reject() => Status = "Rejected";
}
