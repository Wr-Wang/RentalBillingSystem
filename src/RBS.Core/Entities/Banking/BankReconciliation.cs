using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Banking;

/// <summary>
/// 银行对账会话 — 一个对账周期
/// 对账是指将企业内部的收付款记录与银行提供的流水进行核对，
/// 确保账实相符。每个对账会话覆盖一段日期范围。
/// 状态流转：InProgress（进行中）→ Completed（已完成）/ Cancelled（已取消）
/// </summary>
public class BankReconciliation : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 所属公司标识
    /// 对账会话按公司隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 对账开始日期
    /// 本次对账周期的起始日期（含）
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// 对账结束日期
    /// 本次对账周期的结束日期（含）
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// 对账状态
    /// InProgress（进行中）— 正在对账操作中；
    /// Completed（已完成）— 对账结束，结果已确认；
    /// Cancelled（已取消）— 对账被取消作废
    /// </summary>
    public string Status { get; private set; } = "InProgress";

    /// <summary>
    /// 期初余额
    /// 对账周期起始日的银行账户余额（来自银行记录）
    /// </summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>
    /// 期末余额
    /// 对账周期结束日的银行账户余额（来自银行记录）
    /// </summary>
    public decimal ClosingBalance { get; private set; }

    /// <summary>
    /// 银行流水合计金额
    /// 对账周期内所有银行流水的净发生额合计
    /// </summary>
    public decimal StatementTotal { get; private set; }

    /// <summary>
    /// 系统单据合计金额
    /// 对账周期内所有内部收/付款单据的净发生额合计
    /// </summary>
    public decimal SystemTotal { get; private set; }

    /// <summary>
    /// 差异金额
    /// 银行流水合计减去系统单据合计的差额，零表示账实相符
    /// </summary>
    public decimal Difference => StatementTotal - SystemTotal;

    /// <summary>
    /// 对账完成时间（UTC）
    /// 记录对账完成的时刻，仅在 Status 为 Completed 时有值
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private BankReconciliation() { }

    /// <summary>
    /// 创建银行对账会话实例，初始状态为 InProgress（进行中）
    /// </summary>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="startDate">对账开始日期时间</param>
    /// <param name="endDate">对账结束日期时间</param>
    /// <param name="openingBalance">期初余额</param>
    /// <param name="closingBalance">期末余额</param>
    public BankReconciliation(Guid companyId, DateTime startDate, DateTime endDate,
        decimal openingBalance, decimal closingBalance)
    {
        CompanyId = companyId;
        StartDate = startDate;
        EndDate = endDate;
        OpeningBalance = openingBalance;
        ClosingBalance = closingBalance;
    }

    /// <summary>
    /// 设置银行流水合计和系统单据合计金额
    /// 用于计算差异（Difference），判断账实是否相符
    /// </summary>
    /// <param name="statementTotal">银行流水合计金额</param>
    /// <param name="systemTotal">系统单据合计金额</param>
    public void SetTotals(decimal statementTotal, decimal systemTotal)
    {
        StatementTotal = statementTotal;
        SystemTotal = systemTotal;
    }

    /// <summary>
    /// 完成对账
    /// 将对账状态标记为 Completed，记录完成时间
    /// </summary>
    public void Complete()
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 取消对账
    /// 将对账状态标记为 Cancelled，作废本次对账会话
    /// </summary>
    public void Cancel()
    {
        Status = "Cancelled";
    }
}
