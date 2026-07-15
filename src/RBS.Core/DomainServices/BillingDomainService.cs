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
    /// 遍历合同下所有费用配置，筛选在账期内有效的配置项，
    /// 为每个符合条件的费用项目生成一条 Journal 条目。
    /// </summary>
    public List<Journal> GenerateJournals(Contract contract, string period, DateOnly dueDate,
        Guid companyId, Guid defaultSubjectId, DateTime billedAt)
    {
        if (contract.Status != "Active")
            throw new InvalidOperationException("只有生效中的合同才能生成出账记录");

        var journals = new List<Journal>();

        foreach (var feeConfig in contract.FeeConfigs)
        {
            if (!IsFeeEffectiveForPeriod(feeConfig, period))
                continue;

            var journal = new Journal(
                companyId, contract.Id, feeConfig.FeeCodeId, null,
                defaultSubjectId, period, feeConfig.Amount, dueDate,
                "Normal", billedAt, null, null, null);

            journals.Add(journal);
        }

        return journals;
    }

    private static bool IsFeeEffectiveForPeriod(ContractFeeConfig feeConfig, string period)
    {
        var periodStart = DateOnly.Parse($"{period}-01");
        var periodEnd = periodStart.AddDays(DateTime.DaysInMonth(periodStart.Year, periodStart.Month) - 1);

        if (feeConfig.EffectiveDate != null)
        {
            var eff = DateOnly.Parse(feeConfig.EffectiveDate);
            if (periodEnd < eff) return false;
        }
        if (feeConfig.ExpiryDate != null)
        {
            var exp = DateOnly.Parse(feeConfig.ExpiryDate);
            if (periodStart > exp) return false;
        }
        return true;
    }

    /// <summary>
    /// 计算利息（滞纳金）。基于 Journal 余额和配置计算逾期费用。
    /// </summary>
    public decimal CalculateLateFee(decimal amount, decimal received, DateOnly dueDate,
        string status, LateFeeConfig config, DateOnly asOfDate)
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
}
