namespace RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Base;

/// <summary>
/// 会计分录 — Voucher 聚合下的子实体
/// 每笔分录对应一个科目的借方或贷方金额，是凭证的核心组成部分
/// 一个凭证包含至少两条分录（一借一贷），构成完整的记账记录
/// </summary>
public class JournalEntry : AuditableEntity
{
    /// <summary>
    /// 所属凭证标识
    /// 外键关联到 Voucher 聚合根，表示该分录归属的记账凭证
    /// </summary>
    public Guid VoucherId { get; private set; }

    /// <summary>
    /// 会计科目标识
    /// 关联 AccountingSubject，指明该分录所使用的会计科目（如"管理费用-租金"）
    /// </summary>
    public Guid AccountingSubjectId { get; private set; }

    /// <summary>
    /// 借贷方向
    /// 固定值为 "Debit"（借方）或 "Credit"（贷方）
    /// 借方表示资产增加/负债减少，贷方表示资产减少/负债增加
    /// </summary>
    public string Direction { get; private set; }

    /// <summary>
    /// 分录金额（必须大于零）
    /// 借方金额合计必须等于贷方金额合计，以保证借贷平衡
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// 分录摘要说明
    /// 对本笔分录的业务内容进行简要描述，辅助财务人员理解账务含义
    /// </summary>
    public string? Summary { get; private set; }

    /// <summary>
    /// 科目编码（SQL 查询映射用，非持久化）
    /// 通过 JOIN AccountingSubject 表后由 Dapper/SQL 查询直接填充，
    /// 不在 EF Core 中持久化，仅用于列表展示和导出
    /// </summary>
    public string? SubjectCode { get; internal set; }

    /// <summary>
    /// 科目名称（SQL 查询映射用，非持久化）
    /// 通过 JOIN AccountingSubject 表后由 Dapper/SQL 查询直接填充，
    /// 不在 EF Core 中持久化，仅用于列表展示和导出
    /// </summary>
    public string? SubjectName { get; internal set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private JournalEntry() : base()
    {
        Direction = string.Empty;
    }

    /// <summary>
    /// 创建会计分录实例
    /// </summary>
    /// <param name="voucherId">所属凭证标识</param>
    /// <param name="accountingSubjectId">会计科目标识</param>
    /// <param name="direction">借贷方向，必须为 "Debit"（借方）或 "Credit"（贷方）</param>
    /// <param name="amount">分录金额，必须大于零</param>
    /// <exception cref="ArgumentException">当方向不是 Debit/Credit 或金额小于等于零时抛出</exception>
    public JournalEntry(Guid voucherId, Guid accountingSubjectId, string direction, decimal amount) : base()
    {
        if (direction != "Debit" && direction != "Credit")
            throw new ArgumentException("方向必须为 Debit 或 Credit");
        if (amount <= 0) throw new ArgumentException("金额必须大于0");

        VoucherId = voucherId;
        AccountingSubjectId = accountingSubjectId;
        Direction = direction;
        Amount = amount;
    }

    /// <summary>
    /// 设置分录摘要
    /// </summary>
    /// <param name="summary">摘要内容，传入 null 或空白字符串将被清空</param>
    public void SetSummary(string? summary)
    {
        Summary = summary?.Trim();
    }
}
