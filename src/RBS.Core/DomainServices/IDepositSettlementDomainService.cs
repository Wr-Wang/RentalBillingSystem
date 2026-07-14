namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Billing;

/// <summary>
/// 押金结算领域服务 — 合同终止时的押金处理策略（退还/扣除/抵欠费）
/// </summary>
public interface IDepositSettlementDomainService
{
    /// <summary>
    /// 计算押金结算方案
    /// </summary>
    /// <param name="depositBalance">当前押金余额</param>
    /// <param name="outstandingBalance">欠费金额（含应收和滞纳金）</param>
    /// <param name="returnOption">退还选项：FullRefund/DeductArrears/DeductAndReturn</param>
    /// <returns>结算方案</returns>
    DepositSettlementPlan CalculateSettlement(decimal depositBalance, decimal outstandingBalance, string returnOption);

    /// <summary>
    /// 根据结算方案生成押金凭证分录
    /// </summary>
    /// <param name="voucher">待填充分录的凭证</param>
    /// <param name="plan">结算方案</param>
    /// <param name="subjectMap">科目映射</param>
    void ApplySettlementEntries(Voucher voucher, DepositSettlementPlan plan, IReadOnlyDictionary<string, Guid> subjectMap);
}

/// <summary>押金结算方案</summary>
public class DepositSettlementPlan
{
    /// <summary>应收押金退还金额</summary>
    public decimal RefundAmount { get; set; }
    /// <summary>抵扣欠费金额</summary>
    public decimal DeductAmount { get; set; }
    /// <summary>实际退还金额</summary>
    public decimal ActualRefund => RefundAmount - DeductAmount;
    /// <summary>押金余额全額</summary>
    public decimal TotalDeposit { get; set; }
    /// <summary>剩余欠费（不足抵扣部分）</summary>
    public decimal RemainingArrears { get; set; }
}
