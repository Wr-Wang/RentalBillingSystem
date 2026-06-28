namespace RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Base;

/// <summary>
/// 会计分录 — Voucher 聚合下的子实体
/// 每笔分录对应一个科目的借方或贷方金额
/// </summary>
public class JournalEntry : AuditableEntity
{
    public Guid VoucherId { get; private set; }
    public Guid AccountingSubjectId { get; private set; }
    public string Direction { get; private set; }
    public decimal Amount { get; private set; }
    public string? Summary { get; private set; }

    private JournalEntry() : base()
    {
        Direction = string.Empty;
    }

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

    /// <summary>设置分录摘要</summary>
    public void SetSummary(string? summary)
    {
        Summary = summary?.Trim();
    }
}
