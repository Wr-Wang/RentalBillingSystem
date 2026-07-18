namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同修改请求暂存实体 — 审批通过前暂存修改字段
/// 修改请求审批通过后，由应用服务将变更同步至正式合同表
/// 仅记录有变更的字段，未设置的属性为 null
/// </summary>
public class ContractModifyRequest : AuditableEntity
{
    /// <summary>被修改的合同标识</summary>
    public Guid ContractId { get; private set; }
    /// <summary>新的起租日期，null 表示不变更</summary>
    public DateOnly? StartDate { get; private set; }
    /// <summary>新的到期日期，null 表示不变更</summary>
    public DateOnly? EndDate { get; private set; }
    /// <summary>新的付款周期，null 表示不变更</summary>
    public string? PaymentCycle { get; private set; }
    /// <summary>新的自动续签标志，null 表示不变更</summary>
    public bool? AutoRenew { get; private set; }
    /// <summary>新的允许押金抵最后租金标志，null 表示不变更</summary>
    public bool? AllowDepositAsLastRent { get; private set; }
    /// <summary>新的付款到期日，null 表示不变更</summary>
    public int? PaymentDueDay { get; private set; }
    /// <summary>新的租客联系电话，null 表示不变更</summary>
    public string? TenantPhone { get; private set; }
    /// <summary>备注信息</summary>
    public string? Remark { get; private set; }
    /// <summary>请求状态：Draft（草稿）/ PendingApproval（待审批）/ Completed（已完成）/ Rejected（已驳回）</summary>
    public string Status { get; private set; } = "Draft";
    /// <summary>关联的审批请求标识</summary>
    public Guid? ApprovalRequestId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private ContractModifyRequest() { }

    /// <summary>
    /// 创建合同修改请求
    /// </summary>
    /// <param name="contractId">被修改的合同标识</param>
    public ContractModifyRequest(Guid contractId)
    {
        ContractId = contractId;
        Status = "Draft";
    }

    /// <summary>
    /// 批量设置待修改的字段值
    /// </summary>
    /// <param name="startDate">新的起租日期，null 表示不变更</param>
    /// <param name="endDate">新的到期日期，null 表示不变更</param>
    /// <param name="paymentCycle">新的付款周期，null 表示不变更</param>
    /// <param name="autoRenew">新的自动续签标志，null 表示不变更</param>
    /// <param name="allowDepositAsLastRent">新的允许押金抵最后租金标志，null 表示不变更</param>
    /// <param name="paymentDueDay">新的付款到期日，null 表示不变更</param>
    /// <param name="tenantPhone">新的租客联系电话，null 表示不变更</param>
    /// <param name="remark">备注信息，null 表示不变更</param>
    public void SetField(DateOnly? startDate, DateOnly? endDate, string? paymentCycle, bool? autoRenew,
        bool? allowDepositAsLastRent, int? paymentDueDay, string? tenantPhone, string? remark)
    {
        StartDate = startDate; EndDate = endDate; PaymentCycle = paymentCycle;
        AutoRenew = autoRenew; AllowDepositAsLastRent = allowDepositAsLastRent;
        PaymentDueDay = paymentDueDay; TenantPhone = tenantPhone; Remark = remark;
    }

    /// <summary>提交审批，状态变更为 PendingApproval</summary>
    public void Submit() => Status = "PendingApproval";
    /// <summary>审批通过，状态变更为 Completed</summary>
    public void Complete() => Status = "Completed";
    /// <summary>驳回请求，状态变更为 Rejected</summary>
    public void Reject() => Status = "Rejected";
    /// <summary>关联审批请求</summary>
    public void SetApprovalRequestId(Guid approvalRequestId) => ApprovalRequestId = approvalRequestId;
}
