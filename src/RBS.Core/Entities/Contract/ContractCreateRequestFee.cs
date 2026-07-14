namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同创建请求暂存费用配置 — ContractCreateRequest 的子实体
/// 暂存新建合同申请中的费用项，审批通过后创建 ContractFeeConfig 正式记录
/// </summary>
public class ContractCreateRequestFee : AuditableEntity
{
    /// <summary>所属合同创建请求标识</summary>
    public Guid RequestId { get; private set; }
    /// <summary>费用项目标识，指向 FeeCode 字典表</summary>
    public Guid FeeCodeId { get; private set; }
    /// <summary>金额（固定金额模式下使用）</summary>
    public decimal Amount { get; private set; }
    /// <summary>计费模式：FixedAmount（固定金额）/ MeterBased（抄表计量）</summary>
    public string BillingMode { get; private set; } = "FixedAmount";
    /// <summary>收费类型：Recurring（周期性）/ OneTime（一次性）</summary>
    public string ChargeType { get; private set; } = "Recurring";
    /// <summary>计量单位，抄表计量模式下使用，如"吨"、"度"</summary>
    public string? Unit { get; private set; }
    /// <summary>单价，抄表计量模式下使用</summary>
    public decimal? UnitPrice { get; private set; }
    /// <summary>生效日期（yyyy-MM-dd），null 表示立即生效</summary>
    public string? EffectiveDate { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private ContractCreateRequestFee() { }

    /// <summary>
    /// 创建合同创建请求的费用配置
    /// </summary>
    /// <param name="requestId">所属合同创建请求标识</param>
    /// <param name="feeCodeId">费用项目标识</param>
    /// <param name="amount">金额</param>
    /// <param name="billingMode">计费模式</param>
    /// <param name="chargeType">收费类型</param>
    /// <param name="effectiveDate">生效日期，null 表示立即生效</param>
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
