namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class DebitNoteItem : AssociationEntity
{
    public Guid DebitNoteId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public decimal Amount { get; private set; }
    private DebitNoteItem() { }
    public DebitNoteItem(Guid debitNoteId, Guid feeCodeId, decimal amount)
    { DebitNoteId = debitNoteId; FeeCodeId = feeCodeId; Amount = amount; }
}
