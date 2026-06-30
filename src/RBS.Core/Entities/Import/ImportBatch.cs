using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>导入批次 — 每次批量导入生成一个批次，审批通过后执行创建</summary>
public class ImportBatch : AggregateRoot, IHasCompany
{
    public Guid CompanyId { get; set; }
    /// <summary>导入类型标识，如 "HousingUnit"</summary>
    public string ImportType { get; set; } = string.Empty;
    /// <summary>原始文件名</summary>
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int FailedRows { get; set; }
    /// <summary>PendingApproval | Approved | Rejected</summary>
    public string Status { get; set; } = "PendingApproval";
    /// <summary>关联的审批请求 ID</summary>
    public Guid? ApprovalRequestId { get; set; }

    /// <summary>导入行明细</summary>
    public List<ImportBatchItem> Items { get; set; } = new();

    private ImportBatch() : base() { }

    public ImportBatch(Guid companyId, string importType, string fileName)
    {
        CompanyId = companyId;
        ImportType = importType;
        FileName = fileName;
        Status = "PendingApproval";
    }
}
