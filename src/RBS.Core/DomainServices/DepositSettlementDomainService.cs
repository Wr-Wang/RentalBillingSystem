namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Accounting;

/// <summary>
/// 押金结算领域服务实现 — 封装押金退还/扣除/抵欠费的计算规则
/// </summary>
public class DepositSettlementDomainService : IDepositSettlementDomainService
{
    public DepositSettlementPlan CalculateSettlement(decimal depositBalance, decimal outstandingBalance, string returnOption)
    {
        var plan = new DepositSettlementPlan { TotalDeposit = depositBalance };

        switch (returnOption)
        {
            case "FullRefund":
                plan.RefundAmount = depositBalance;
                plan.DeductAmount = 0;
                plan.RemainingArrears = outstandingBalance;
                break;

            case "DeductArrears":
                plan.DeductAmount = Math.Min(depositBalance, outstandingBalance);
                plan.RefundAmount = depositBalance;
                plan.RemainingArrears = outstandingBalance - plan.DeductAmount;
                break;

            case "DeductAndReturn":
                plan.DeductAmount = Math.Min(depositBalance, outstandingBalance);
                plan.RefundAmount = depositBalance - plan.DeductAmount;
                plan.RemainingArrears = Math.Max(0, outstandingBalance - plan.DeductAmount);
                break;

            default:
                plan.RefundAmount = depositBalance;
                plan.DeductAmount = 0;
                plan.RemainingArrears = outstandingBalance;
                break;
        }

        return plan;
    }

    public void ApplySettlementEntries(Voucher voucher, DepositSettlementPlan plan, IReadOnlyDictionary<string, Guid> subjectMap)
    {
        if (!subjectMap.TryGetValue("112202", out var depositArId))
            throw new ArgumentException("科目映射缺少「112202-应收押金」");
        if (!subjectMap.TryGetValue("1001", out var cashId))
            throw new ArgumentException("科目映射缺少「1001-库存现金」");

        // 冲销应收押金（全额）
        voucher.AddEntry(depositArId, "Credit", plan.TotalDeposit, "合同终止-冲销押金");

        // 退还部分：借库存现金
        if (plan.ActualRefund > 0)
            voucher.AddEntry(cashId, "Debit", plan.ActualRefund, "押金退还");

        // 抵扣部分：借相关费用科目
        if (plan.DeductAmount > 0)
        {
            if (subjectMap.TryGetValue("6001", out var revenueId))
                voucher.AddEntry(revenueId, "Debit", plan.DeductAmount, "押金抵扣欠费");
        }
    }
}
