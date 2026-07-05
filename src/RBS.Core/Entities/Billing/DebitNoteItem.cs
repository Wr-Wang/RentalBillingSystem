namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 账单明细行 — 记录单条费用项目的应收情况，含费用名称快照
/// </summary>
public class DebitNoteItem : AssociationEntity
{
    public Guid DebitNoteId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public string? FeeName { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Received { get; private set; }
    public string? BillingMode { get; private set; }
    public string? Unit { get; private set; }

    private DebitNoteItem() : base() { }

    /// <summary>创建账单明细</summary>
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

    public void SetSnapshot(string? feeName, string? billingMode, string? unit)
    {
        FeeName = feeName;
        BillingMode = billingMode;
        Unit = unit;
    }

    public void RecordPayment(decimal amount)
    {
        Received += amount;
    }
}
