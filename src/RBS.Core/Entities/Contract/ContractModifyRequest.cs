namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同修改请求暂存实体 — 审批通过前暂存修改字段
/// </summary>
public class ContractModifyRequest : AuditableEntity
{
    public Guid ContractId { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? PaymentCycle { get; private set; }
    public bool? AutoRenew { get; private set; }
    public bool? AllowDepositAsLastRent { get; private set; }
    public int? PaymentDueDay { get; private set; }
    public string? TenantPhone { get; private set; }
    public string? Remark { get; private set; }
    public string Status { get; private set; } = "Draft";
    public Guid? ApprovalRequestId { get; private set; }

    private ContractModifyRequest() { }

    public ContractModifyRequest(Guid contractId)
    {
        ContractId = contractId;
        Status = "Draft";
    }

    public void SetField(DateOnly? startDate, DateOnly? endDate, string? paymentCycle, bool? autoRenew,
        bool? allowDepositAsLastRent, int? paymentDueDay, string? tenantPhone, string? remark)
    {
        StartDate = startDate; EndDate = endDate; PaymentCycle = paymentCycle;
        AutoRenew = autoRenew; AllowDepositAsLastRent = allowDepositAsLastRent;
        PaymentDueDay = paymentDueDay; TenantPhone = tenantPhone; Remark = remark;
    }

    public void Submit() => Status = "PendingApproval";
    public void Complete() => Status = "Completed";
    public void Reject() => Status = "Rejected";
}
