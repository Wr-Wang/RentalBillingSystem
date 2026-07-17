namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.SystemConfig;

/// <summary>
/// 计费领域服务接口 — Journal 出账生成和利息计算。
/// </summary>
public interface IBillingDomainService
{
    /// <summary>为合同批量生成指定账期的 Journal 出账记录。</summary>
    List<Journal> GenerateJournals(
        List<(Guid FeeCodeId, decimal Amount, string? EffectiveDate, string? ExpiryDate)> feeConfigs,
        Guid contractId, Guid companyId, string period, DateOnly dueDate,
        Guid defaultSubjectId, DateTime billedAt);

    /// <summary>计算利息（滞纳金）。</summary>
    decimal CalculateLateFee(decimal amount, decimal received, DateOnly dueDate,
        string status, LateFeeConfig config, DateOnly asOfDate);

    /// <summary>按天分摊金额。</summary>
    decimal CalculateProratedAmount(decimal monthlyAmount, int daysInPeriod, int occupiedDays);

    /// <summary>按天分摊生成 Journal。</summary>
    List<Journal> GenerateProratedJournals(
        List<(Guid FeeCodeId, decimal Amount, string? EffectiveDate, string? ExpiryDate, string FeeName)> feeConfigs,
        Guid contractId, string period, DateOnly dueDate,
        Guid companyId, Guid defaultSubjectId, DateTime billedAt);

    /// <summary>计算周期收费的月份拆分方案。</summary>
    List<FeeMonthSegment> CalculateMonthlySplit(string effectiveDate, DateTime now);
}
