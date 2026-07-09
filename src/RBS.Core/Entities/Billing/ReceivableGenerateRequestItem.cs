namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 生成应收请求暂存明细 — 各月各费用分摊数据
/// </summary>
public class ReceivableGenerateRequestItem : AuditableEntity
{
    public Guid RequestId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public string FeeName { get; private set; } = string.Empty;
    public string Period { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string EntryType { get; private set; } = "Normal";
    public Guid? ReceivablePlanId { get; private set; }
    public Guid? VoucherId { get; private set; }

    private ReceivableGenerateRequestItem() { }

    public ReceivableGenerateRequestItem(Guid requestId, Guid feeCodeId, string feeName,
        string period, decimal amount, DateOnly dueDate, string entryType = "Normal")
    {
        RequestId = requestId; FeeCodeId = feeCodeId; FeeName = feeName;
        Period = period; Amount = amount; DueDate = dueDate; EntryType = entryType;
    }

    public void SetPlanIds(Guid receivablePlanId, Guid voucherId)
    {
        ReceivablePlanId = receivablePlanId; VoucherId = voucherId;
    }
}
