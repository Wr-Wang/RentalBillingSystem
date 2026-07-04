using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Core.DomainServices;

/// <summary>
/// 计费领域服务 — 应收生成和滞纳金计算
/// </summary>
public class BillingDomainService : IBillingDomainService
{
    public List<ReceivablePlan> GenerateReceivablePlans(Contract contract, string period, DateOnly dueDate)
    {
        if (contract.Status != "Active")
            throw new InvalidOperationException("只有生效中的合同才能生成应收计划");

        var plans = new List<ReceivablePlan>();

        foreach (var feeConfig in contract.FeeConfigs.Where(f => f.IsActive))
        {
            // ★ v3: 费用版本化过滤 — 账期必须在 EffectiveDate ~ ExpiryDate 范围内
            if (!IsFeeEffectiveForPeriod(feeConfig, period))
                continue;

            decimal amount = feeConfig.Amount;
            // TODO: 首月或末月按天分摊计算，需根据合同起止日期和账期判断

            var plan = new ReceivablePlan(
                contract.Id,
                feeConfig.FeeCodeId,
                period,
                amount,
                dueDate);

            plans.Add(plan);
        }

        return plans;
    }

    /// <summary>判断费用配置在指定账期是否有效</summary>
    private static bool IsFeeEffectiveForPeriod(ContractFeeConfig feeConfig, string period)
    {
        // EffectiveDate 不为空且账期早于生效日期 → 跳过
        if (feeConfig.EffectiveDate != null &&
            string.Compare(period, feeConfig.EffectiveDate.Substring(0, 7), StringComparison.Ordinal) < 0)
            return false;

        // ExpiryDate 不为空且账期晚于到期日期 → 跳过
        if (feeConfig.ExpiryDate != null &&
            string.Compare(period, feeConfig.ExpiryDate.Substring(0, 7), StringComparison.Ordinal) > 0)
            return false;

        return true;
    }

    public decimal CalculateLateFee(ReceivablePlan plan, LateFeeConfig config, DateOnly asOfDate)
    {
        if (plan.Status != "Pending" && plan.Status != "Partial" && plan.Status != "Overdue")
            return 0;

        if (asOfDate <= plan.DueDate)
            return 0;

        var daysOverdue = asOfDate.DayNumber - plan.DueDate.DayNumber;
        var effectiveDays = Math.Max(0, daysOverdue - config.GraceDays);

        if (effectiveDays <= 0) return 0;

        var overdueBalance = plan.Amount - plan.Received;
        if (overdueBalance <= 0) return 0;

        var fee = overdueBalance * config.DailyRate * effectiveDays;

        if (config.MaxRate.HasValue && config.MaxRate.Value > 0)
        {
            var maxFee = overdueBalance * config.MaxRate.Value / 100;
            fee = Math.Min(fee, maxFee);
        }

        return Math.Round(fee, 2);
    }

    public decimal CalculateProratedAmount(decimal monthlyAmount, int daysInPeriod, int occupiedDays)
    {
        if (daysInPeriod <= 0) return 0;
        return Math.Round(monthlyAmount * occupiedDays / daysInPeriod, 2);
    }
}
