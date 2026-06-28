namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class ReceiptAllocation : AssociationEntity
{
    public Guid ReceiptId { get; private set; }
    public Guid ReceivablePlanId { get; private set; }
    public decimal Amount { get; private set; }
    private ReceiptAllocation() { }
    public ReceiptAllocation(Guid receiptId, Guid receivablePlanId, decimal amount)
    { ReceiptId = receiptId; ReceivablePlanId = receivablePlanId; Amount = amount; }
}
