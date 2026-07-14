namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 账单明细行实体（继承 AssociationEntity）
/// —— 记录单条费用项目在账单中的应收情况，含费用名称快照字段。
/// 一个 <see cref="DebitNote"/> 包含多个 DebitNoteItem，分别对应不同的费用项目。
/// </summary>
public class DebitNoteItem : AssociationEntity
{
    /// <summary>所属账单 ID，指向 <see cref="DebitNote"/> 的主键</summary>
    public Guid DebitNoteId { get; private set; }

    /// <summary>费用项目 ID，指向 <see cref="FeeCode"/> 的主键</summary>
    public Guid FeeCodeId { get; private set; }

    /// <summary>费用名称（快照字段），出账时定格，后续 FeeCode 名称变更不影响历史账单</summary>
    public string? FeeName { get; private set; }

    /// <summary>应收金额，单位：元。创建时确定不可变更。</summary>
    public decimal Amount { get; private set; }

    /// <summary>已收金额，单位：元。通过 RecordPayment 累计。</summary>
    public decimal Received { get; private set; }

    /// <summary>计费方式（快照字段）：FixedAmount（固定金额）| Metered（按表计量）| RateByArea（按面积）</summary>
    public string? BillingMode { get; private set; }

    /// <summary>计量单位（快照字段），如 "吨"、"度"、"平方米"</summary>
    public string? Unit { get; private set; }

    /// <summary>私有无参构造函数，供 EF Core 延迟加载使用</summary>
    private DebitNoteItem() : base() { }

    /// <summary>
    /// 创建账单明细行。
    /// </summary>
    /// <param name="debitNoteId">所属账单 ID</param>
    /// <param name="feeCodeId">费用项目 ID</param>
    /// <param name="amount">应收金额，必须大于 0</param>
    /// <param name="feeName">费用名称（快照，可 null）</param>
    /// <param name="billingMode">计费方式（快照，可 null）</param>
    /// <param name="unit">计量单位（快照，可 null）</param>
    /// <exception cref="ArgumentException">amount 小于等于 0 时抛出</exception>
    public DebitNoteItem(Guid debitNoteId, Guid feeCodeId, decimal amount,
        string? feeName = null, string? billingMode = null, string? unit = null)
    {
        if (amount <= 0)
            throw new ArgumentException("金额必须大于0", nameof(amount));
        DebitNoteId = debitNoteId;
        FeeCodeId = feeCodeId;
        Amount = amount;
        FeeName = feeName;
        BillingMode = billingMode;
        Unit = unit;
    }

    /// <summary>设置费用信息快照（出账时定格，后续变更不影响历史账单）</summary>
    /// <param name="feeName">费用名称</param>
    /// <param name="billingMode">计费方式</param>
    /// <param name="unit">计量单位</param>
    public void SetSnapshot(string? feeName, string? billingMode, string? unit)
    {
        FeeName = feeName;
        BillingMode = billingMode;
        Unit = unit;
    }

    /// <summary>记录一笔收款分配至此明细项，增加已收金额</summary>
    /// <param name="amount">本次收款金额</param>
    public void RecordPayment(decimal amount)
    {
        Received += amount;
    }
}
