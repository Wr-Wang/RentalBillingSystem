using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>导入批次 — 每次批量导入生成一个批次，审批通过后执行创建</summary>
public class ImportBatch : AggregateRoot, IHasCompany
{
    public Guid CompanyId { get; private set; }
    public string ImportType { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public int TotalRows { get; private set; }
    public int ValidRows { get; private set; }
    public int FailedRows { get; private set; }
    public string Status { get; private set; } = "PendingApproval";
    public Guid? ApprovalRequestId { get; private set; }

    private readonly List<ImportBatchItem> _items = new();
    public IReadOnlyCollection<ImportBatchItem> Items => _items.AsReadOnly();

    private ImportBatch() : base() { }

    public ImportBatch(Guid companyId, string importType, string fileName)
    {
        CompanyId = companyId;
        ImportType = importType;
        FileName = fileName;
        Status = "PendingApproval";
    }

    public void AddItem(ImportBatchItem item)
    {
        _items.Add(item);
        TotalRows = _items.Count;
    }

    public void SetRowCounts(int valid, int failed)
    {
        ValidRows = valid;
        FailedRows = failed;
    }

    public void Approve()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的批次不能审批");
        Status = "Approved";
    }

    public void Reject()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的批次不能驳回");
        Status = "Rejected";
    }

    public void LoadItems(IEnumerable<ImportBatchItem> items) { _items.Clear(); _items.AddRange(items); }
    public void SetApprovalRequest(Guid? approvalRequestId)
    {
        ApprovalRequestId = approvalRequestId;
    }
}
