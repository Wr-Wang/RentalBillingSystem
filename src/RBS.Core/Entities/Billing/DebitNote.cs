namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class DebitNote : AuditableEntity
{
    public string NoteNo { get; private set; } = string.Empty;
    public Guid ContractId { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; } = "Draft";
    private readonly List<DebitNoteItem> _items = new();
    public IReadOnlyCollection<DebitNoteItem> Items => _items.AsReadOnly();

    private DebitNote() { }
    public DebitNote(string noteNo, Guid contractId, string period)
    {
        if (string.IsNullOrWhiteSpace(noteNo)) throw new ArgumentException("账单编号不能为空", nameof(noteNo));
        NoteNo = noteNo; ContractId = contractId; Period = period;
    }
    public void SetTotalAmount(decimal total) { TotalAmount = total; }
    public void LoadItems(IEnumerable<DebitNoteItem> items) { _items.Clear(); _items.AddRange(items); }
}
