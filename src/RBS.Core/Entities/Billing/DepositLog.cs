namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 押金日志实体（领域实体，继承 AuditableEntity）
/// —— 记录合同押金的完整生命周期操作，包括押金创建、退还、扣除等。
/// 每笔操作都会生成一条不可变的日志记录，用于审计追溯。
/// 操作类型（Action）："Create"（创建）、"Return"（退还）、"Deduct"（扣除）。
/// 注意：Amount 在退还/扣除操作为负值，Balance 始终为操作后的实时余额。
/// </summary>
public class DepositLog : AuditableEntity
{
    /// <summary>关联的合同 ID，标识该押金属于哪个合同</summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// 操作金额。正数表示押金创建（增加），负数表示退还或扣除（减少）。
    /// 例如：创建时为 +5000.00，退还时为 -2000.00。
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>操作后的押金余额，始终 >= 0。用于快速查询当前押金状态而不需要汇总计算。</summary>
    public decimal Balance { get; private set; }

    /// <summary>
    /// 操作类型。
    /// "Create"（创建）—— 新建押金记录；
    /// "Return"（退还）—— 退还押金给租户；
    /// "Deduct"（扣除）—— 因违约等原因扣除部分押金。
    /// </summary>
    public string Action { get; private set; } = "Create";

    /// <summary>操作备注，说明退还/扣除的原因，例如 "合同到期全额退还"、"扣除维修费用"</summary>
    public string? Remark { get; private set; }

    /// <summary>私有无参构造函数，供 EF Core 延迟加载使用</summary>
    private DepositLog() { }

    /// <summary>
    /// 创建押金日志（Action = "Create"）。
    /// 首次创建押金时调用，金额为正数，余额等于金额。
    /// </summary>
    /// <param name="contractId">关联的合同 ID</param>
    /// <param name="amount">押金金额，必须大于 0，单位：元</param>
    public DepositLog(Guid contractId, decimal amount)
    {
        ContractId = contractId;
        Amount = amount;
        Balance = amount;
        Action = "Create";
    }

    /// <summary>
    /// 创建退还押金日志（Action = "Return"）。
    /// 生成 Amount 为负值的日志记录，余额自动减少。
    /// </summary>
    /// <param name="contractId">关联的合同 ID</param>
    /// <param name="amount">退还金额，必须大于 0（内部会转为负值记录）</param>
    /// <param name="currentBalance">当前押金余额，用于校验和计算退还后余额</param>
    /// <param name="remark">退还原因备注，例如 "合同到期全额退还"</param>
    /// <returns>退还押金日志实例</returns>
    /// <exception cref="ArgumentException">当 amount 小于等于 0 或超过 currentBalance 时抛出</exception>
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

    /// <summary>
    /// 创建扣除押金日志（Action = "Deduct"）。
    /// 生成 Amount 为负值的日志记录，余额自动减少。
    /// 适用于租户违约、设施损坏赔偿等场景。
    /// </summary>
    /// <param name="contractId">关联的合同 ID</param>
    /// <param name="amount">扣除金额，必须大于 0（内部会转为负值记录）</param>
    /// <param name="currentBalance">当前押金余额，用于校验和计算扣除后余额</param>
    /// <param name="remark">扣除原因备注，例如 "扣除维修费用 500 元"</param>
    /// <returns>扣除押金日志实例</returns>
    /// <exception cref="ArgumentException">当 amount 小于等于 0 或超过 currentBalance 时抛出</exception>
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
