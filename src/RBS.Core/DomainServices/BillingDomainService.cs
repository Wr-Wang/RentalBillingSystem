using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Core.DomainServices;

/// <summary>
/// 计费领域服务 — 应收生成和滞纳金计算。
/// 实现 IBillingDomainService 接口，提供计费周期内的核心计算逻辑，
/// 包括批量应收计划生成、按费用有效日期的分段分摊、滞纳金计算等。
/// </summary>
public class BillingDomainService : IBillingDomainService
{
    /// <summary>
    /// 为合同批量生成指定账期的应收计划。
    /// 遍历合同下所有费用配置，筛选在账期内有效的配置项，
    /// 为每个符合条件的费用项目生成一条应收计划。
    /// 仅对"Active"状态的合同生效。
    /// </summary>
    /// <param name="contract">目标合同聚合根，必须为"Active"状态</param>
    /// <param name="period">账期，格式为"yyyy-MM"</param>
    /// <param name="dueDate">应收到期日</param>
    /// <returns>应收计划列表</returns>
    /// <exception cref="InvalidOperationException">合同不是生效状态时抛出</exception>
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

    /// <summary>
    /// 计算滞纳金。
    /// 基于应收计划余额和滞纳金配置计算逾期费用。
    /// 支持宽限期（GraceDays）、日费率（DailyRate）和最高费率上限（MaxRate）。
    /// 仅对 Pending/Partial/Overdue 状态的计划有滞纳金，
    /// 且在未超过宽限期时返回 0。
    /// </summary>
    /// <param name="plan">应收计划，包含金额、已收金额和到期日信息</param>
    /// <param name="config">滞纳金配置规则</param>
    /// <param name="asOfDate">计算截止日期</param>
    /// <returns>滞纳金金额，保留两位小数；无滞纳金时返回 0</returns>
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

    /// <summary>
    /// 按天分摊金额。
    /// 根据月度金额、账期天数和实际占用天数计算比例分摊金额。
    /// </summary>
    /// <param name="monthlyAmount">月度标准金额</param>
    /// <param name="daysInPeriod">账期总天数</param>
    /// <param name="occupiedDays">实际占用天数</param>
    /// <returns>按天分摊后的金额，保留两位小数</returns>
    public decimal CalculateProratedAmount(decimal monthlyAmount, int daysInPeriod, int occupiedDays)
    {
        if (daysInPeriod <= 0) return 0;
        return Math.Round(monthlyAmount * occupiedDays / daysInPeriod, 2);
    }

    /// <summary>
    /// 按天分摊生成应收计划 — 同一费用项目在同一个月内有多条配置时逐段分摊后汇总。
    /// 处理费用版本化场景：某费用在同一个月内发生价格调整时，
    /// 按各自的生效日期区间分别计算占用天数，分摊累计后作为最终应收金额。
    /// </summary>
    /// <param name="feeConfigs">费用配置元组列表（FeeCodeId、金额、生效日、到期日、费用名称）</param>
    /// <param name="contractId">合同 ID</param>
    /// <param name="period">账期，格式"yyyy-MM"</param>
    /// <param name="dueDate">应收到期日</param>
    /// <returns>按费用项目汇总后的应收计划列表</returns>
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
