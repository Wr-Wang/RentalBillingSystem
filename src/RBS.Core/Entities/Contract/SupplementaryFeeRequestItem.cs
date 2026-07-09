namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 补充收费请求暂存明细 — 各月分摊数据
/// </summary>
public class SupplementaryFeeRequestItem : AuditableEntity
{
    public Guid RequestId { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public decimal ProratedAmount { get; private set; }
    public int DaysInMonth { get; private set; }
    public int CoveredDays { get; private set; }
    public Guid? ReceivablePlanId { get; private set; }
    public Guid? VoucherId { get; private set; }

    private SupplementaryFeeRequestItem() { }

    public SupplementaryFeeRequestItem(Guid requestId, string period, decimal proratedAmount,
        int daysInMonth, int coveredDays)
    {
        RequestId = requestId; Period = period; ProratedAmount = proratedAmount;
        DaysInMonth = daysInMonth; CoveredDays = coveredDays;
    }

    public void SetPlanIds(Guid receivablePlanId, Guid voucherId)
    {
        ReceivablePlanId = receivablePlanId; VoucherId = voucherId;
    }
}
