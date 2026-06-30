using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>导入行明细基础实体 — 所有导入类型共用的字段</summary>
public class ImportBatchItem : AuditableEntity
{
    public Guid ImportBatchId { get; set; }
    /// <summary>鉴别器列，如 "HousingUnit"</summary>
    public string ImportType { get; set; } = string.Empty;
    /// <summary>Excel 行号</summary>
    public int RowIndex { get; set; }
    /// <summary>格式校验是否通过</summary>
    public bool IsValid { get; set; }
    /// <summary>错误码</summary>
    public string? ErrorCode { get; set; }
    /// <summary>错误描述</summary>
    public string? ErrorMessage { get; set; }
    /// <summary>修正建议</summary>
    public string? FixSuggestion { get; set; }

    /// <summary>所属批次</summary>
    public ImportBatch? Batch { get; set; }

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
