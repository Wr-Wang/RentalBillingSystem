namespace RBS.Core.Entities.Accounting;

using RBS.Core.Entities.Base;

/// <summary>
/// 总账余额（GeneralLedgerBalance）—— 按期间汇总的应收活动
///
/// DDD 角色：领域实体（Entity），继承 AuditableEntity，实现 IHasCompany。
/// 一条记录概括该期间的所有应收活动：期初 + 出账 - 收款 = 期末。
/// 不做科目级拆分，科目信息保留在 Journal 明细中。
///
/// 计算公式：ClosingBalance = OpeningBalance + TotalBilled - TotalReceived
/// </summary>
public class GeneralLedgerBalance : AuditableEntity, IHasCompany
{
    /// <summary>所属公司ID</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>会计期间 yyyy-MM</summary>
    public string Period { get; private set; } = string.Empty;

    /// <summary>期初应收余额（上期期末结转）</summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>本期出账总额（汇总 Journal.Amount）</summary>
    public decimal TotalBilled { get; private set; }

    /// <summary>本期收款总额（汇总 ReceiptAllocation.Amount）</summary>
    public decimal TotalReceived { get; private set; }

    /// <summary>期末应收余额（= OpeningBalance + TotalBilled - TotalReceived）</summary>
    public decimal ClosingBalance { get; private set; }

    /// <summary>最后计算时间</summary>
    public DateTime LastCalculatedAt { get; private set; }

    /// <summary>私有无参构造，仅供 Dapper 反序列化使用</summary>
    private GeneralLedgerBalance() { }

    /// <summary>
    /// 创建总账余额记录
    /// </summary>
    public GeneralLedgerBalance(Guid companyId, string period)
    {
        CompanyId = companyId;
        Period = period;
        OpeningBalance = 0;
        TotalBilled = 0;
        TotalReceived = 0;
        ClosingBalance = 0;
        LastCalculatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 添加本期出账金额（出账时调用）
    /// </summary>
    public void AddBilled(decimal amount)
    {
        TotalBilled += amount;
        Recalculate();
    }

    /// <summary>
    /// 添加本期收款金额（收款确认时调用）
    /// </summary>
    public void AddReceived(decimal amount)
    {
        TotalReceived += amount;
        Recalculate();
    }

    /// <summary>
    /// 重新计算期末余额
    /// </summary>
    private void Recalculate()
    {
        ClosingBalance = OpeningBalance + TotalBilled - TotalReceived;
        LastCalculatedAt = DateTime.UtcNow;
    }
}
