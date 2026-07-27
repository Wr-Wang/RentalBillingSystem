using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Banking;

/// <summary>
/// 银行流水行 — 从银行导入的每笔交易记录
/// 表示银行对账单中的一条明细，包含交易日期、金额、余额、对方账户等信息。
/// 状态流转：Unmatched（未匹配）→ Matched（已匹配）→ Reconciled（已对账）
/// </summary>
public class BankStatement : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 所属公司标识
    /// 银行流水按公司隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 交易日期
    /// 银行流水中的交易发生日期（注意：可能与内部系统记账日期不一致）
    /// </summary>
    public DateTime TransactionDate { get; private set; }

    /// <summary>
    /// 交易金额
    /// 正数表示收入（入账），负数表示支出（出账）
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// 交易后余额
    /// 银行记录的该笔交易完成后的账户余额，用于核对账户连续性
    /// </summary>
    public decimal Balance { get; private set; }

    /// <summary>
    /// 交易摘要/备注
    /// 银行提供的交易描述信息，如"工资发放"、"租金收入"等
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 银行流水号/参考号
    /// 银行生成的唯一交易标识号，用于匹配和对账（如银企直连流水号）
    /// </summary>
    public string? ReferenceNo { get; private set; }

    /// <summary>
    /// 对方账户信息
    /// 交易对手的账户名称或账号，用于识别交易对方
    /// </summary>
    public string? Counterparty { get; private set; }

    /// <summary>
    /// 匹配状态
    /// Unmatched（未匹配）— 刚导入，尚未与内部单据关联；
    /// Matched（已匹配）— 已与内部收款/付款单据关联；
    /// Reconciled（已对账）— 对账周期内已确认一致
    /// </summary>
    public string Status { get; private set; } = "Unmatched";

    /// <summary>
    /// 导入批次标识
    /// 记录该流水是从哪个导入批次来的，便于追溯和批量处理
    /// </summary>
    public Guid? ImportBatchId { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private BankStatement() { }

    /// <summary>
    /// 创建银行流水行实例，初始状态为 Unmatched（未匹配）
    /// </summary>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="transactionDate">交易日期</param>
    /// <param name="amount">交易金额（正=收入，负=支出）</param>
    /// <param name="balance">交易后余额</param>
    public BankStatement(Guid companyId, DateTime transactionDate, decimal amount, decimal balance)
    {
        CompanyId = companyId;
        TransactionDate = transactionDate;
        Amount = amount;
        Balance = balance;
    }

    /// <summary>
    /// 设置参考信息
    /// 补充银行流水号、摘要描述和对方账户信息，通常在导入解析后调用
    /// </summary>
    /// <param name="refNo">银行流水号</param>
    /// <param name="description">交易摘要</param>
    /// <param name="counterparty">对方账户信息</param>
    public void SetReference(string? refNo, string? description, string? counterparty)
    {
        ReferenceNo = refNo;
        Description = description;
        Counterparty = counterparty;
    }

    /// <summary>
    /// 标记为已匹配
    /// 表示该笔流水已与内部单据（收款单/付款单）关联成功
    /// </summary>
    public void MarkMatched() => Status = "Matched";

    /// <summary>
    /// 标记为已对账
    /// 表示在银行对账周期中已确认该笔流水一致无误
    /// </summary>
    public void MarkReconciled() => Status = "Reconciled";
}
