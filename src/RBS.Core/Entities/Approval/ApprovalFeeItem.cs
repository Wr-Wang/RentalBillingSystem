using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Approval;

/// <summary>
/// 审批调价明细 — 费用调价审批中每个费用项的逐条记录
/// 1:N 关联 ApprovalRequest
/// </summary>
public class ApprovalFeeItem : AuditableEntity
{
    public Guid ApprovalRequestId { get; private set; }
    public Guid ContractId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public string FeeName { get; private set; }
    public decimal OldAmount { get; private set; }
    public decimal NewAmount { get; private set; }

    /// <summary>计费模式：FixedAmount / MeterBased</summary>
    public string BillingMode { get; private set; }

    /// <summary>计量单位（MeterBased 模式使用）</summary>
    public string? Unit { get; private set; }

    /// <summary>生效日期（每条费用独立，yyyy-MM-dd）</summary>
    public string? EffectiveDate { get; private set; }

    private ApprovalFeeItem() : base()
    {
        FeeName = string.Empty;
        BillingMode = "FixedAmount";
    }

    public ApprovalFeeItem(Guid approvalRequestId, Guid contractId, Guid feeCodeId, string feeName,
        decimal oldAmount, decimal newAmount, string billingMode, string? unit, string? effectiveDate = null) : base()
    {
        ApprovalRequestId = approvalRequestId;
        ContractId = contractId;
        FeeCodeId = feeCodeId;
        FeeName = feeName;
        OldAmount = oldAmount;
        NewAmount = newAmount;
        BillingMode = billingMode;
        Unit = unit;
        EffectiveDate = effectiveDate;
    }
}
