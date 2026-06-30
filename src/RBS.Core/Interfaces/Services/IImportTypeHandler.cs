using RBS.Core.Entities.Import;

namespace RBS.Core.Interfaces.Services;

/// <summary>导入类型处理器 — 每种导入类型实现一个</summary>
public interface IImportTypeHandler
{
    string ImportType { get; }

    /// <summary>获取该导入类型对应的审批类型 ID</summary>
    Task<Guid> GetApprovalTypeIdAsync(Guid companyId);

    /// <summary>解析 JsonElement 数据转为 ImportBatchItem 派生实体，并做校验</summary>
    ImportBatchItem ParseAndValidate(Guid importBatchId, int rowIndex, System.Text.Json.JsonElement data, ImportValidationContext context);

    /// <summary>审批通过后执行业务创建，返回创建数</summary>
    Task<int> ExecuteAsync(ImportBatch batch, CancellationToken ct);
}

/// <summary>校验上下文（由 ImportService 构建）</summary>
public class ImportValidationContext
{
    public Guid CompanyId { get; set; }
    public HashSet<string> ExistingKeys { get; set; } = new();
    public HashSet<string> BatchKeys { get; set; } = new();
    public Dictionary<string, object> CustomData { get; set; } = new();
}
