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

    /// <summary>
    /// 按天分摊生成应收计划 — 同一费用项目在同一个月内有多条配置时逐段分摊后汇总
    /// </summary>
    public List<ReceivablePlan> GenerateProratedReceivablePlans(
        List<(Guid FeeCodeId, decimal Amount, string? EffectiveDate, string? ExpiryDate, string FeeName)> feeConfigs,
        Guid contractId, string period, DateOnly dueDate)
    {
        var periodParts = period.Split('-');
        var year = int.Parse(periodParts[0]);
        var month = int.Parse(periodParts[1]);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var periodStart = new DateOnly(year, month, 1);
        var periodEnd = new DateOnly(year, month, daysInMonth);

        var groups = feeConfigs.GroupBy(f => f.FeeCodeId);
        var plans = new List<ReceivablePlan>();

        foreach (var group in groups)
        {
            decimal totalAmount = 0;

            foreach (var config in group)
            {
                var effStart = config.EffectiveDate != null
                    ? DateOnly.Parse(config.EffectiveDate)
                    : periodStart;
                var effEnd = config.ExpiryDate != null
                    ? DateOnly.Parse(config.ExpiryDate)
                    : periodEnd;

                var overlapStart = effStart > periodStart ? effStart : periodStart;
                var overlapEnd = effEnd < periodEnd ? effEnd : periodEnd;

                var coveredDays = overlapStart <= overlapEnd
                    ? overlapEnd.DayNumber - overlapStart.DayNumber + 1
                    : 0;

                if (coveredDays > 0)
                {
                    var prorated = Math.Round(config.Amount / daysInMonth * coveredDays, 2);
                    totalAmount += prorated;
                }
            }

            totalAmount = Math.Round(totalAmount, 2);
            var plan = new ReceivablePlan(contractId, group.Key, period, totalAmount, dueDate);
            plans.Add(plan);
        }

        return plans;
    }
}
