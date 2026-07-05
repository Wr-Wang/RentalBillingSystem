namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 账单聚合根 — 出账时由 BillJob 生成，含快照字段（后续合同信息变更不影响历史账单）
/// </summary>
public class DebitNote : AuditableEntity
{
    public string NoteNo { get; private set; } = string.Empty;
    public Guid ContractId { get; private set; }
    public string? ContractNo { get; private set; }
    public string? Period { get; private set; }
    public Guid CompanyId { get; private set; }

    // === 快照字段（出账时定格，后续变更不影响历史） ===
    public string? RoomFullCode { get; private set; }
    public string? TenantName { get; private set; }

    public decimal TotalAmount { get; private set; }
    public decimal TotalReceived { get; private set; }
    public decimal TotalPrepaid { get; private set; }
    public decimal BalanceDue { get; private set; }

    public string Status { get; private set; } = "Draft";
    public bool IsHistorical { get; private set; }
    public DateOnly? DueDate { get; private set; }

    public DateTime? GeneratedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledBy { get; private set; }
    public string? CancelReason { get; private set; }
    public Guid? BillJobTaskLogId { get; private set; }

    private readonly List<DebitNoteItem> _items = new();
    public IReadOnlyCollection<DebitNoteItem> Items => _items.AsReadOnly();

    private DebitNote() : base() { }

    public DebitNote(string noteNo, Guid contractId, string period)
    {
        if (string.IsNullOrWhiteSpace(noteNo))
            throw new ArgumentException("账单编号不能为空", nameof(noteNo));
        NoteNo = noteNo;
        ContractId = contractId;
        Period = period;
    }

    public void SetTotalAmount(decimal total) { TotalAmount = total; }
    public void SetSnapshot(string? contractNo, string? roomCode, string? tenantName)
    {
        ContractNo = contractNo;
        RoomFullCode = roomCode;
        TenantName = tenantName;
    }
    public void SetPaymentSummary(decimal received, decimal prepaid)
    {
        TotalReceived = received;
        TotalPrepaid = prepaid;
        BalanceDue = TotalAmount - TotalReceived - TotalPrepaid;
    }
    public void MarkGenerated(Guid taskLogId, bool isHistorical = false, DateOnly? dueDate = null)
    {
        Status = "Published";
        GeneratedAt = RBS.Core.Common.ChinaTime.Now;
        BillJobTaskLogId = taskLogId;
        IsHistorical = isHistorical;
        DueDate = dueDate;
    }
    public void Cancel(Guid userId, string reason)
    {
        Status = "Cancelled";
        CancelledAt = RBS.Core.Common.ChinaTime.Now;
        CancelledBy = userId;
        CancelReason = reason;
    }
    public void LoadItems(IEnumerable<DebitNoteItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }
}
