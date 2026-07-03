using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>导入行明细基础实体 — 所有导入类型共用的字段</summary>
public class ImportBatchItem : AuditableEntity
{
    public Guid ImportBatchId { get; private set; }
    public string ImportType { get; private set; } = string.Empty;
    public int RowIndex { get; private set; }
    public bool IsValid { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? FixSuggestion { get; private set; }

    protected ImportBatchItem() : base() { }

    public ImportBatchItem(Guid importBatchId, string importType, int rowIndex)
    {
        ImportBatchId = importBatchId;
        ImportType = importType;
        RowIndex = rowIndex;
        IsValid = true;
    }

    public void SetFailed(string errorCode, string errorMessage, string? fixSuggestion = null)
    {
        IsValid = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        FixSuggestion = fixSuggestion;
    }
}
