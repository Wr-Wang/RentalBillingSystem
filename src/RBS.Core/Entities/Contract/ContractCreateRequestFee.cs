namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同创建请求暂存费用配置
/// </summary>
public class ContractCreateRequestFee : AuditableEntity
{
    public Guid RequestId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public decimal Amount { get; private set; }
    public string BillingMode { get; private set; } = "FixedAmount";
    public string ChargeType { get; private set; } = "Recurring";
    public string? Unit { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string? EffectiveDate { get; private set; }

    private ContractCreateRequestFee() { }

    public ContractCreateRequestFee(Guid requestId, Guid feeCodeId, decimal amount, string billingMode, string chargeType, string? effectiveDate = null)
    {
        RequestId = requestId;
        FeeCodeId = feeCodeId;
        Amount = amount;
        BillingMode = billingMode;
        ChargeType = chargeType;
        EffectiveDate = effectiveDate;
    }
}
