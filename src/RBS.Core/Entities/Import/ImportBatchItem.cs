using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>
/// 导入行明细基础实体 — 所有导入类型共用的字段
/// 记录导入文件中每一行数据的校验结果，支持 TPH（Table-Per-Hierarchy）继承，
/// 子类通过 ImportType 区分具体的导入业务类型
/// </summary>
public class ImportBatchItem : AuditableEntity
{
    /// <summary>
    /// 所属导入批次标识，关联到 ImportBatch 聚合根
    /// </summary>
    public Guid ImportBatchId { get; private set; }

    /// <summary>
    /// 导入类型，标识该行数据的业务对象类型（如 "HousingUnit"），用于 TPH 继承鉴别
    /// </summary>
    public string ImportType { get; private set; } = string.Empty;

    /// <summary>
    /// 数据行号（从 0 开始计数，通常 0=表头行，1=第一条数据），用于定位和提示用户
    /// </summary>
    public int RowIndex { get; private set; }

    /// <summary>
    /// 该行数据是否校验通过。true=有效（可导入），false=校验失败
    /// 默认值为 true
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// 错误码（仅 IsValid=false 时有值），用于分类汇总错误类型
    /// </summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// 错误描述（仅 IsValid=false 时有值），说明校验失败的具体原因
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// 修复建议（可选），为用户提供修改方向的说明
    /// </summary>
    public string? FixSuggestion { get; private set; }

    /// <summary>
    /// 受保护的构造函数，仅用于 EF Core 反序列化或子类继承调用
    /// </summary>
    protected ImportBatchItem() : base() { }

    /// <summary>
    /// 创建导入行明细实例。创建时默认标记为有效（IsValid=true）
    /// </summary>
    /// <param name="importBatchId">所属批次标识</param>
    /// <param name="importType">导入类型，用于 TPH 鉴别</param>
    /// <param name="rowIndex">数据行号</param>
    public ImportBatchItem(Guid importBatchId, string importType, int rowIndex)
    {
        ImportBatchId = importBatchId;
        ImportType = importType;
        RowIndex = rowIndex;
        IsValid = true;
    }

    /// <summary>
    /// 将该行标记为校验失败，记录错误信息
    /// </summary>
    /// <param name="errorCode">错误码，用于错误分类</param>
    /// <param name="errorMessage">错误描述</param>
    /// <param name="fixSuggestion">修复建议（可选）</param>
    public void SetFailed(string errorCode, string errorMessage, string? fixSuggestion = null)
    {
        IsValid = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        FixSuggestion = fixSuggestion;
    }
}
