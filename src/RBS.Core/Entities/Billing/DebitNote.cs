namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class DebitNote : AuditableEntity
{
    public string NoteNo { get; private set; } = string.Empty;
    public Guid ContractId { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; } = "Draft";
    private DebitNote() { }
    public DebitNote(string noteNo, Guid contractId, string period)
    { NoteNo = noteNo; ContractId = contractId; Period = period; }
    public ICollection<DebitNoteItem> Items { get; private set; } = new List<DebitNoteItem>();
}
