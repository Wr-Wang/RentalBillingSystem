namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 生成应收请求暂存实体 — 手动生成应收审批通过前暂存
/// </summary>
public class ReceivableGenerateRequest : AuditableEntity, IHasCompany
{
    public Guid ContractId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string PeriodFrom { get; private set; } = string.Empty;
    public string PeriodTo { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Draft";
    public Guid? ApprovalRequestId { get; private set; }

    private ReceivableGenerateRequest() { }

    public ReceivableGenerateRequest(Guid contractId, Guid companyId, string periodFrom, string periodTo)
    {
        ContractId = contractId; CompanyId = companyId;
        PeriodFrom = periodFrom; PeriodTo = periodTo; Status = "Draft";
    }

    public void Submit() => Status = "PendingApproval";
    public void Complete() => Status = "Completed";
    public void Reject() => Status = "Rejected";
}
