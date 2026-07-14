using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>
/// 导入批次 — 每次批量导入生成一个聚合根实例
/// 代表一次完整的导入操作：上传文件 -> 暂存 -> 审批 -> 执行创建。
/// 包含导入类型、文件信息、行数据状态统计及审批流关联。
/// 实现了 IHasCompany 接口以支持多租户隔离
/// </summary>
public class ImportBatch : AggregateRoot, IHasCompany
{
    /// <summary>
    /// 所属公司标识，用于多租户数据隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 导入类型，标识此次导入的业务对象类型，如 "HousingUnit"（房源）、"Contract"（合同）
    /// </summary>
    public string ImportType { get; private set; } = string.Empty;

    /// <summary>
    /// 上传的文件名称（含扩展名），用于追溯和审计
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// 导入总行数（包含表头），等于导入行明细的条目数
    /// </summary>
    public int TotalRows { get; private set; }

    /// <summary>
    /// 有效行数，通过校验的数据行数
    /// </summary>
    public int ValidRows { get; private set; }

    /// <summary>
    /// 失败行数，校验不通过的数据行数
    /// </summary>
    public int FailedRows { get; private set; }

    /// <summary>
    /// 导入批次状态。
    /// PendingApproval=待审批, Approved=已审批通过, Rejected=已驳回
    /// 默认值为 "PendingApproval"
    /// </summary>
    public string Status { get; private set; } = "PendingApproval";

    /// <summary>
    /// 关联的审批请求标识（可选），当批次提交审批后关联到具体的审批流
    /// </summary>
    public Guid? ApprovalRequestId { get; private set; }

    /// <summary>
    /// 导入行明细集合（只读后端字段）
    /// </summary>
    private readonly List<ImportBatchItem> _items = new();

    /// <summary>
    /// 获取导入行明细的只读集合
    /// </summary>
    public IReadOnlyCollection<ImportBatchItem> Items => _items.AsReadOnly();

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ImportBatch() : base() { }

    /// <summary>
    /// 创建导入批次实例。创建后状态为待审批（PendingApproval）
    /// </summary>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="importType">导入类型，如 "HousingUnit"、"Contract"</param>
    /// <param name="fileName">上传文件名，含扩展名</param>
    public ImportBatch(Guid companyId, string importType, string fileName)
    {
        CompanyId = companyId;
        ImportType = importType;
        FileName = fileName;
        Status = "PendingApproval";
    }

    /// <summary>
    /// 添加一条导入行明细到批次中
    /// </summary>
    /// <param name="item">导入行明细实体</param>
    public void AddItem(ImportBatchItem item)
    {
        _items.Add(item);
        TotalRows = _items.Count;
    }

    /// <summary>
    /// 设置导入数据的有效行数和失败行数（通常由校验引擎计算后调用）
    /// </summary>
    /// <param name="valid">有效行数</param>
    /// <param name="failed">失败行数</param>
    public void SetRowCounts(int valid, int failed)
    {
        ValidRows = valid;
        FailedRows = failed;
    }

    /// <summary>
    /// 审批通过该导入批次。仅在待审批状态下可操作
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不是 PendingApproval 时抛出</exception>
    public void Approve()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的批次不能审批");
        Status = "Approved";
    }

    /// <summary>
    /// 驳回该导入批次。仅在待审批状态下可操作
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不是 PendingApproval 时抛出</exception>
    public void Reject()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的批次不能驳回");
        Status = "Rejected";
    }

    /// <summary>
    /// 批量加载导入行明细（先清空后加载，用于持久化恢复场景）
    /// </summary>
    /// <param name="items">导入行明细集合</param>
    public void LoadItems(IEnumerable<ImportBatchItem> items) { _items.Clear(); _items.AddRange(items); }

    /// <summary>
    /// 设置关联的审批请求标识
    /// </summary>
    /// <param name="approvalRequestId">审批请求标识，设为 null 表示取消关联</param>
    public void SetApprovalRequest(Guid? approvalRequestId)
    {
        ApprovalRequestId = approvalRequestId;
    }
}
