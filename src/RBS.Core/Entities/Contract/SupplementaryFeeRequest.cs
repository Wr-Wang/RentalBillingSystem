namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 补充收费请求暂存实体 — 审批通过前暂存补充收费数据
/// </summary>
public class SupplementaryFeeRequest : AuditableEntity, IHasCompany
{
    public Guid ContractId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public decimal Amount { get; private set; }
    public string BillingMode { get; private set; } = "FixedAmount";
    public string EffectiveDate { get; private set; } = string.Empty;
    public string PeriodFrom { get; private set; } = string.Empty;
    public string PeriodTo { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Draft";
    public Guid? ApprovalRequestId { get; private set; }
    public Guid? FeeConfigId { get; private set; }
    public Guid CompanyId { get; private set; }

    private SupplementaryFeeRequest() { }

    public SupplementaryFeeRequest(Guid contractId, Guid feeCodeId, decimal amount, string effectiveDate,
        string periodFrom, string periodTo, Guid companyId)
    {
        ContractId = contractId; FeeCodeId = feeCodeId; Amount = amount;
        EffectiveDate = effectiveDate; PeriodFrom = periodFrom; PeriodTo = periodTo;
        CompanyId = companyId; Status = "Draft";
    }

    public void Submit() => Status = "PendingApproval";
    public void Complete(Guid feeConfigId) { FeeConfigId = feeConfigId; Status = "Completed"; }
    public void Reject() => Status = "Rejected";
}
