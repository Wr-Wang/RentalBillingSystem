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

        foreach (var feeConfig in contract.FeeConfigs)
        {
            // ★ v3: 费用版本化过滤 — 仅根据日期区间判断，不依赖 IsActive（历史配置也需要参与应收计算）
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

    /// <summary>判断费用配置在指定账期是否有效（按完整日期判断，不依赖 IsActive）</summary>
    private static bool IsFeeEffectiveForPeriod(ContractFeeConfig feeConfig, string period)
    {
        var periodStart = DateOnly.Parse($"{period}-01");
        var periodEnd = periodStart.AddDays(DateTime.DaysInMonth(periodStart.Year, periodStart.Month) - 1);

        // 账期结束日 < 生效日 → 不收费
        if (feeConfig.EffectiveDate != null)
        {
            var eff = DateOnly.Parse(feeConfig.EffectiveDate);
            if (periodEnd < eff) return false;
        }

        // 账期起始日 > 到期日 → 不收费
        if (feeConfig.ExpiryDate != null)
        {
            var exp = DateOnly.Parse(feeConfig.ExpiryDate);
            if (periodStart > exp) return false;
        }

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
