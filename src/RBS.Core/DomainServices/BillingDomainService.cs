using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Core.DomainServices;

/// <summary>
/// 计费领域服务 — Journal 生成和利息计算。
/// </summary>
public class BillingDomainService : IBillingDomainService
{
    /// <summary>
    /// 为合同批量生成指定账期的 Journal（日记账出账记录）。
    /// 遍历费用配置列表，筛选在账期内有效的配置项，
    /// 为每个符合条件的费用项目生成一条 Journal 条目。
    /// </summary>
    public List<Journal> GenerateJournals(
        List<(Guid FeeCodeId, decimal Amount, string? EffectiveDate, string? ExpiryDate)> feeConfigs,
        Guid contractId, Guid companyId, string period, DateOnly dueDate,
        Guid defaultSubjectId, DateTime billedAt)
    {
        var journals = new List<Journal>();

        foreach (var fc in feeConfigs)
        {
            if (!IsFeeEffectiveForPeriod(fc.EffectiveDate, fc.ExpiryDate, period))
                continue;

            var journal = new Journal(
                companyId, contractId, fc.FeeCodeId, null,
                defaultSubjectId, period, fc.Amount, dueDate,
                "Normal", billedAt, null, null, null);

            journals.Add(journal);
        }

        return journals;
    }

    private static bool IsFeeEffectiveForPeriod(string? effectiveDate, string? expiryDate, string period)
    {
        var periodStart = DateOnly.Parse($"{period}-01");
        var periodEnd = periodStart.AddDays(DateTime.DaysInMonth(periodStart.Year, periodStart.Month) - 1);

        if (effectiveDate != null)
        {
            var eff = DateOnly.Parse(effectiveDate);
            if (periodEnd < eff) return false;
        }
        if (expiryDate != null)
        {
            var exp = DateOnly.Parse(expiryDate);
            if (periodStart > exp) return false;
        }
        return true;
    }

    /// <summary>
    /// 计算利息（利息）。基于 Journal 余额和配置计算逾期费用。
    /// </summary>
    public decimal CalculateInterest(decimal amount, decimal received, DateOnly dueDate,
        string status, InterestConfig config, DateOnly asOfDate)
    {
        if (status == "Paid" || status == "Cancelled")
            return 0;
        if (asOfDate <= dueDate)
            return 0;

        var daysOverdue = asOfDate.DayNumber - dueDate.DayNumber;
        var effectiveDays = Math.Max(0, daysOverdue - config.GraceDays);
        if (effectiveDays <= 0) return 0;

        var balance = amount - received;
        if (balance <= 0) return 0;

        var fee = balance * config.DailyRate * effectiveDays;
        if (config.MaxRate.HasValue && config.MaxRate.Value > 0)
        {
            var maxFee = balance * config.MaxRate.Value / 100;
            fee = Math.Min(fee, maxFee);
        }
        return Math.Round(fee, 2);
    }

    /// <summary>
    /// 按天分摊金额。
    /// </summary>
    public decimal CalculateProratedAmount(decimal monthlyAmount, int daysInPeriod, int occupiedDays)
    {
        if (daysInPeriod <= 0) return 0;
        return Math.Round(monthlyAmount * occupiedDays / daysInPeriod, 2);
    }

    /// <summary>
    /// 按天分摊生成 Journal — 同一费用项目在同一个月内有多条配置时逐段分摊后汇总。
    /// </summary>
    public List<Journal> GenerateProratedJournals(
        List<(Guid FeeCodeId, decimal Amount, string? EffectiveDate, string? ExpiryDate, string FeeName)> feeConfigs,
        Guid contractId, string period, DateOnly dueDate,
        Guid companyId, Guid defaultSubjectId, DateTime billedAt)
    {
        var periodParts = period.Split('-');
        var year = int.Parse(periodParts[0]);
        var month = int.Parse(periodParts[1]);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var periodStart = new DateOnly(year, month, 1);
        var periodEnd = new DateOnly(year, month, daysInMonth);

        var groups = feeConfigs.GroupBy(f => f.FeeCodeId);
        var journals = new List<Journal>();

        foreach (var group in groups)
        {
            decimal totalAmount = 0;
            foreach (var config in group)
            {
                var effStart = config.EffectiveDate != null
                    ? DateOnly.Parse(config.EffectiveDate) : periodStart;
                var effEnd = config.ExpiryDate != null
                    ? DateOnly.Parse(config.ExpiryDate) : periodEnd;
                var overlapStart = effStart > periodStart ? effStart : periodStart;
                var overlapEnd = effEnd < periodEnd ? effEnd : periodEnd;
                var coveredDays = overlapStart <= overlapEnd
                    ? overlapEnd.DayNumber - overlapStart.DayNumber + 1 : 0;
                if (coveredDays > 0)
                    totalAmount += Math.Round(config.Amount / daysInMonth * coveredDays, 2);
            }
            totalAmount = Math.Round(totalAmount, 2);
            var journal = new Journal(companyId, contractId, group.Key, null,
                defaultSubjectId, period, totalAmount, dueDate,
                "Normal", billedAt, null, null, null);
            journals.Add(journal);
        }
        return journals;
    }

    /// <summary>
    /// 计算周期收费的月份拆分方案。
    /// 从 effectiveDate 到当前月份逐月生成独立分段，每个分段覆盖一个自然月。
    /// 首段（分摊期）的 Amount 按天折算，中间月/未来月使用全额。
    /// 纯计算逻辑，不涉及任何持久化。
    /// contractStartDate/contractEndDate 用于兜底裁剪生效日期至合同期内（防御性校验）。
    /// </summary>
    /// <param name="monthlyAmount">月度全额</param>
    /// <param name="effectiveDate">费用生效日（yyyy-MM-dd）</param>
    /// <param name="now">当前时间</param>
    /// <param name="contractStartDate">合同起租日期</param>
    /// <param name="contractEndDate">合同到期日期，null 表示长期</param>
    /// <returns>拆分后的分段列表</returns>
    /// <example>
    /// 输入：monthlyAmount=4500, effectiveDate="2026-05-20", now=2026-07-17
    /// 输出：4 个分段：5/20~5/31(1741.94), 6/1~6/30(4500), 7/1~7/31(4500), 8/1~NULL(4500)
    /// </example>
    public List<FeeMonthSegment> CalculateMonthlySplit(decimal monthlyAmount, string effectiveDate, DateTime now,
        DateOnly contractStartDate, DateOnly? contractEndDate)
    {
        var effDate = DateOnly.Parse(effectiveDate);

        // 兜底裁剪：生效日期不能早于合同起租日
        if (effDate < contractStartDate)
            effDate = contractStartDate;

        // 兜底裁剪：生效日期不能晚于合同到期日
        if (contractEndDate.HasValue && effDate > contractEndDate.Value)
            return new List<FeeMonthSegment>();

        var effMonth = new DateOnly(effDate.Year, effDate.Month, 1);
        var currentMonth = new DateOnly(now.Year, now.Month, 1);
        var nextMonth = currentMonth.AddMonths(1);
        var segments = new List<FeeMonthSegment>();

        // 生效日在当前月之后 → 单条，不拆分
        if (effMonth > currentMonth)
        {
            // 裁剪后检查是否超出合同到期日
            if (contractEndDate.HasValue && effDate > contractEndDate.Value)
                return segments;

            segments.Add(new FeeMonthSegment
            {
                EffectiveDate = effDate.ToString("yyyy-MM-dd"),
                ExpiryDate = null,
                IsActive = true,
                Amount = monthlyAmount
            });
            return segments;
        }

        // 逐月拆分：从生效月循环到当前月
        var cursor = effMonth;
        while (cursor <= currentMonth)
        {
            var monthEnd = cursor.AddMonths(1).AddDays(-1);

            // 如果 cursor 月已超出合同到期日，停止拆分
            if (contractEndDate.HasValue && cursor > contractEndDate.Value)
                break;

            var segEffDate = cursor == effMonth ? effDate.ToString("yyyy-MM-dd") : cursor.ToString("yyyy-MM-dd");
            // segEffDate 不应超过合同到期日
            if (contractEndDate.HasValue)
            {
                var segEffParsed = DateOnly.Parse(segEffDate);
                if (segEffParsed > contractEndDate.Value)
                    break;
            }

            var daysInMonth = DateTime.DaysInMonth(cursor.Year, cursor.Month);
            var actMonthEnd = contractEndDate.HasValue && monthEnd > contractEndDate.Value
                ? contractEndDate.Value
                : monthEnd;

            // 首段按天分摊，中间月全额
            decimal segAmount;
            if (cursor == effMonth)
            {
                var segEff = DateOnly.Parse(segEffDate);
                var occupiedDays = actMonthEnd.DayNumber - segEff.DayNumber + 1;
                segAmount = occupiedDays > 0
                    ? Math.Round(monthlyAmount / daysInMonth * occupiedDays, 2)
                    : 0;
            }
            else
            {
                segAmount = monthlyAmount;
            }

            if (segAmount > 0)
            {
                segments.Add(new FeeMonthSegment
                {
                    EffectiveDate = segEffDate,
                    ExpiryDate = actMonthEnd.ToString("yyyy-MM-dd"),
                    IsActive = false,
                    Amount = segAmount
                });
            }
            cursor = cursor.AddMonths(1);
        }

        // 未来长期配置：下月1日起，无到期日，启用（但不超过合同到期日）
        if (!contractEndDate.HasValue || nextMonth <= DateOnly.FromDateTime(new DateTime(contractEndDate.Value.Year, contractEndDate.Value.Month, 1)))
        {
            segments.Add(new FeeMonthSegment
            {
                EffectiveDate = nextMonth.ToString("yyyy-MM-dd"),
                ExpiryDate = null,
                IsActive = true,
                Amount = monthlyAmount
            });
        }

        return segments;
    }
}
