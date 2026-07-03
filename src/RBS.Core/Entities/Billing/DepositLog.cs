namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 押金日志 — 记录押金创建、退还、扣除等操作
/// </summary>
public class DepositLog : AuditableEntity
{
    public Guid ContractId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Balance { get; private set; }
    public string Action { get; private set; } = "Create";
    public string? Remark { get; private set; }

    private DepositLog() { }

    /// <summary>新建押金（Create）</summary>
    public DepositLog(Guid contractId, decimal amount)
    {
        ContractId = contractId;
        Amount = amount;
        Balance = amount;
        Action = "Create";
    }

    /// <summary>退还押金（Return）</summary>
    public static DepositLog ForReturn(Guid contractId, decimal amount, decimal currentBalance, string? remark = null)
    {
        if (amount <= 0) throw new ArgumentException("退还金额必须大于0", nameof(amount));
        if (amount > currentBalance) throw new ArgumentException("退还金额超过押金余额", nameof(amount));

        return new DepositLog
        {
            ContractId = contractId,
            Amount = -amount,
            Balance = currentBalance - amount,
            Action = "Return",
            Remark = remark
        };
    }

    /// <summary>扣除押金（Deduct）</summary>
    public static DepositLog ForDeduct(Guid contractId, decimal amount, decimal currentBalance, string? remark = null)
    {
        if (amount <= 0) throw new ArgumentException("扣除金额必须大于0", nameof(amount));
        if (amount > currentBalance) throw new ArgumentException("扣除金额超过押金余额", nameof(amount));

        return new DepositLog
        {
            ContractId = contractId,
            Amount = -amount,
            Balance = currentBalance - amount,
            Action = "Deduct",
            Remark = remark
        };
    }
}
