namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 应收生成请求明细实体 — 存储应收生成请求中各月各费用项目的分摊数据。
/// 每个明细项对应一个费用项目在某个月份的应收金额，是 <see cref="ReceivableGenerateRequest"/> 的子项。
/// 继承自 <see cref="AuditableEntity"/>。
/// </summary>
public class ReceivableGenerateRequestItem : AuditableEntity
{
    /// <summary>
    /// 请求ID — 关联的应收生成请求主表标识，指向 <see cref="ReceivableGenerateRequest"/>。
    /// </summary>
    public Guid RequestId { get; private set; }

    /// <summary>
    /// 费用项目ID — 标识此明细项对应的费用项目（如租金、物业费、水电费等）。
    /// </summary>
    public Guid FeeCodeId { get; private set; }

    /// <summary>
    /// 费用名称 — 费用项目的名称冗余字段，便于展示而不必关联查询费用项目定义表。
    /// </summary>
    public string FeeName { get; private set; } = string.Empty;

    /// <summary>
    /// 期间 — 费用归属期间，格式为 "yyyyMM"（如 "202601" 表示 2026年1月）。
    /// </summary>
    public string Period { get; private set; } = string.Empty;

    /// <summary>
    /// 金额 — 该费用项目在指定期间的应收金额，精确到分。
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// 到期日 — 此笔应收款项的约定付款截止日期。
    /// </summary>
    public DateOnly DueDate { get; private set; }

    /// <summary>
    /// 分录类型 — 应收款项的业务类型分类。支持以下值：
    /// "Normal"(正常) — 常规周期性费用；
    /// "Deposit"(押金) — 押金类费用；
    /// "Supplementary"(补录) — 后续补录的费用调整。
    /// 默认值为 "Normal"。
    /// </summary>
    public string EntryType { get; private set; } = "Normal";

    /// <summary>
    /// 日记账ID — 审批通过并正式生成应收后，关联的 Journal 记录标识。
    /// 为 null 时表示尚未正式生成应收。
    /// </summary>
    public Guid? JournalId { get; private set; }

    /// <summary>
    /// 私有无参构造函数 — 供 EF Core 等 ORM 框架使用，禁止外部直接调用。
    /// </summary>
    private ReceivableGenerateRequestItem() { }

    /// <summary>
    /// 初始化应收生成请求明细实体。
    /// </summary>
    /// <param name="requestId">请求ID，关联的应收生成请求主表标识。</param>
    /// <param name="feeCodeId">费用项目ID，标识费用类型。</param>
    /// <param name="feeName">费用名称，费用项目的可读名称。</param>
    /// <param name="period">费用归属期间，格式 "yyyyMM"。</param>
    /// <param name="amount">应收金额，精确到分。</param>
    /// <param name="dueDate">付款到期日。</param>
    /// <param name="entryType">分录类型，"Normal"(正常)、"Deposit"(押金)、"Supplementary"(补录)，默认 "Normal"。</param>
    public ReceivableGenerateRequestItem(Guid requestId, Guid feeCodeId, string feeName,
        string period, decimal amount, DateOnly dueDate, string entryType = "Normal")
    {
        RequestId = requestId; FeeCodeId = feeCodeId; FeeName = feeName;
        Period = period; Amount = amount; DueDate = dueDate; EntryType = entryType;
    }

    /// <summary>
    /// 设置计划关联ID — 在应收审批通过正式生成应收记录后，关联 Journal 记录。
    /// 此方法通常在 <see cref="ReceivableGenerateRequest.Complete"/> 流程中被调用。
    /// </summary>
    /// <param name="journalId">Journal ID，正式生成的日记账记录标识。</param>
    public void SetJournalId(Guid journalId)
    {
        JournalId = journalId;
    }
}
