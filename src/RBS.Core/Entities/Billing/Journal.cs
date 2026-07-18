namespace RBS.Core.Entities.Billing;

using RBS.Core.Entities.Base;

/// <summary>
/// 日记账（Journal）—— 不可变的出账记录
///
/// DDD 角色：领域实体（Entity），继承 AuditableEntity，实现 IHasCompany。
/// 记录每次出账的应收信息，写入即终态，永不修改。
/// 不记录已收金额、不记录余额、没有状态流转。
///
/// 设计要点：
/// - GLPosted 标记是否已写入总账，正常创建时在同事务中置 true
/// - Amount 允许负数，用于红字冲销错误记录
/// - ParentJournalId 指向被冲销/被调整的源日记账
/// - 无 RowVersion（从不 UPDATE）
/// - Updated* 审计字段保留（继承 AuditableEntity），但永不 populate
/// </summary>
public class Journal : AuditableEntity, IHasCompany
{
    /// <summary>所属公司ID</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>合同ID</summary>
    public Guid ContractId { get; private set; }

    /// <summary>费用项目ID（租金/物业费/利息等）</summary>
    public Guid FeeCodeId { get; private set; }

    /// <summary>费用配置实例ID（一次性费用幂等去重用，NULL为周期性费用）</summary>
    public Guid? FeeConfigId { get; private set; }

    /// <summary>会计科目ID（默认1122应收账款）</summary>
    public Guid AccountingSubjectId { get; private set; }

    /// <summary>归属账期 yyyy-MM</summary>
    public string Period { get; private set; } = string.Empty;

    /// <summary>金额（允许负数，用于冲销错误记录）</summary>
    public decimal Amount { get; private set; }

    /// <summary>到期日</summary>
    public DateOnly DueDate { get; private set; }

    /// <summary>
    /// 条目类型
    /// Normal（周期费用）/ Deposit（押金）/ Supplementary（补差）/ Interest（利息）/ Adjustment（手工调整）
    /// </summary>
    public string EntryType { get; private set; } = "Normal";

    /// <summary>是否已写入总账（0=未入账，1=已入账）</summary>
    public bool GLPosted { get; private set; }

    /// <summary>总账写入时间</summary>
    public DateTime? PostedAt { get; private set; }

    /// <summary>出账时间</summary>
    public DateTime BilledAt { get; private set; }

    /// <summary>关联账单ID</summary>
    public Guid? DebitNoteId { get; private set; }

    /// <summary>关联源日记账ID（利息/调整/冲销指向被操作的源记录）</summary>
    public Guid? ParentJournalId { get; private set; }

    /// <summary>摘要说明</summary>
    public string? Summary { get; private set; }

    /// <summary>私有无参构造，仅供 Dapper 反序列化使用</summary>
    private Journal() { }

    /// <summary>
    /// 创建日记账条目
    /// </summary>
    public Journal(
        Guid companyId,
        Guid contractId,
        Guid feeCodeId,
        Guid? feeConfigId,
        Guid accountingSubjectId,
        string period,
        decimal amount,
        DateOnly dueDate,
        string entryType,
        DateTime billedAt,
        Guid? debitNoteId,
        Guid? parentJournalId,
        string? summary)
    {
        CompanyId = companyId;
        ContractId = contractId;
        FeeCodeId = feeCodeId;
        FeeConfigId = feeConfigId;
        AccountingSubjectId = accountingSubjectId;
        Period = period;
        Amount = amount;
        DueDate = dueDate;
        EntryType = entryType;
        BilledAt = billedAt;
        DebitNoteId = debitNoteId;
        ParentJournalId = parentJournalId;
        Summary = summary;
        GLPosted = false;
        PostedAt = null;
    }

    /// <summary>
    /// 标记为已入总账（在 UPSERT GL 成功后调用）
    /// </summary>
    public void MarkAsPosted()
    {
        GLPosted = true;
        PostedAt = DateTime.UtcNow;
    }
}
