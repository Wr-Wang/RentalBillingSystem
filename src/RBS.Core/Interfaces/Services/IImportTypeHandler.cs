using RBS.Core.Entities.Import;

namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 导入类型处理器 — 每种导入类型实现一个此接口。
/// 策略模式的具体实现，用于导入不同类型的业务数据。
/// 每个实现类通过 <see cref="ImportType"/> 标识其处理的导入类型，
/// 由导入引擎根据导入模板配置动态调度。
/// 职责包括：数据解析校验、获取审批类型、执行审批通过后的业务创建。
/// </summary>
public interface IImportTypeHandler
{
    /// <summary>当前处理器负责的导入类型标识（如"Tenant"、"Contract"等）</summary>
    string ImportType { get; }

    /// <summary>
    /// 获取该导入类型对应的审批类型 ID。
    /// 用于在导入审批流程中确定使用的审批级别配置。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <returns>审批类型 ID</returns>
    Task<Guid> GetApprovalTypeIdAsync(Guid companyId);

    /// <summary>
    /// 解析 JsonElement 数据转为 ImportBatchItem 派生实体，并做数据校验。
    /// 将导入模板中的一行原始 JSON 数据解析为具体的业务实体，
    /// 同时执行字段格式校验、必填项检查等验证逻辑。
    /// 解析过程中如发现数据问题，通过 context 收集错误信息。
    /// </summary>
    /// <param name="importBatchId">导入批次 ID</param>
    /// <param name="rowIndex">当前行号（从 0 开始）</param>
    /// <param name="data">原始 JSON 数据</param>
    /// <param name="context">校验上下文，用于传递公司 ID、已有数据键值集合等</param>
    /// <returns>解析后的导入批次明细实体</returns>
    ImportBatchItem ParseAndValidate(Guid importBatchId, int rowIndex, System.Text.Json.JsonElement data, ImportValidationContext context);

    /// <summary>
    /// 审批通过后执行业务创建，返回创建数。
    /// 将导入的临时数据转换为正式的业务实体并持久化。
    /// </summary>
    /// <param name="batch">导入批次信息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>成功创建的记录数</returns>
    Task<int> ExecuteAsync(ImportBatch batch, CancellationToken ct);
}

/// <summary>
/// 校验上下文（由 ImportService 构建）。
/// 封装导入校验过程中所需的辅助数据，包括公司 ID、
/// 已有数据的键集合、当前批次的键集合以及自定义数据字典。
/// </summary>
public class ImportValidationContext
{
    /// <summary>当前操作的公司 ID</summary>
    public Guid CompanyId { get; set; }
    /// <summary>系统中已存在的唯一键集合（用于校验重复）</summary>
    public HashSet<string> ExistingKeys { get; set; } = new();
    /// <summary>当前导入批次中已解析的键集合（用于校验批次内重复）</summary>
    public HashSet<string> BatchKeys { get; set; } = new();
    /// <summary>自定义数据字典，用于在导入步骤间传递额外信息</summary>
    public Dictionary<string, object> CustomData { get; set; } = new();
}
