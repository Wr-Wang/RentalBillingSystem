namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.SystemConfig;

/// <summary>
/// 计费领域服务接口 — 应收计划生成和滞纳金计算。
/// 定义计费周期内的核心计算契约，包括批量应收计划生成、
/// 滞纳金计算以及按天分摊金额计算。
/// </summary>
public interface IBillingDomainService
{
    /// <summary>
    /// 为合同批量生成指定账期的应收计划。
    /// 遍历合同下的所有有效费用配置，为每个符合条件的费用项目生成一条应收计划。
    /// 只有生效中的合同才能生成应收计划。
    /// </summary>
    /// <param name="contract">目标合同聚合根，必须为"Active"状态</param>
    /// <param name="period">账期，格式为"yyyy-MM"</param>
    /// <param name="dueDate">应收到期日</param>
    /// <returns>应收计划列表</returns>
    /// <exception cref="InvalidOperationException">合同不是生效状态时抛出</exception>
    List<ReceivablePlan> GenerateReceivablePlans(Contract contract, string period, DateOnly dueDate);

    /// <summary>
    /// 计算滞纳金。
    /// 基于应收计划余额、滞纳金配置规则和计算截止日期计算逾期费用。
    /// 支持宽限期、日费率、最高费率上限等参数。
    /// 仅对 Pending、Partial、Overdue 状态的计划计算滞纳金。
    /// </summary>
    /// <param name="plan">应收计划，包含金额和已收金额信息</param>
    /// <param name="config">滞纳金配置（日费率、宽限天数、最高费率上限）</param>
    /// <param name="asOfDate">计算截止日期</param>
    /// <returns>滞纳金金额，若无滞纳金则返回 0</returns>
    decimal CalculateLateFee(ReceivablePlan plan, LateFeeConfig config, DateOnly asOfDate);

    /// <summary>
    /// 按天分摊金额。
    /// 根据月度金额、账期总天数和实际占用天数，计算按比例分摊的金额。
    /// 用于处理首月/末月非完整周期的费用计算场景。
    /// </summary>
    /// <param name="monthlyAmount">月度标准金额</param>
    /// <param name="daysInPeriod">账期总天数</param>
    /// <param name="occupiedDays">实际占用天数</param>
    /// <returns>按天分摊后的金额</returns>
    decimal CalculateProratedAmount(decimal monthlyAmount, int daysInPeriod, int occupiedDays);

    /// <summary>
    /// 按天分摊生成应收计划 — 同一费用项目在同一个月内有多条配置时逐段分摊后汇总。
    /// 用于处理月内调价、合同月中生效/终止等场景的费用计算。
    /// </summary>
    /// <param name="feeConfigs">费用配置列表，每条包含 FeeCodeId、金额、生效日、到期日、费用名称</param>
    /// <param name="contractId">合同标识</param>
    /// <param name="period">账期，格式"yyyy-MM"</param>
    /// <param name="dueDate">应收到期日</param>
    /// <returns>应收计划列表（每个 FeeCodeId 一条汇总计划）</returns>
    List<ReceivablePlan> GenerateProratedReceivablePlans(
        List<(Guid FeeCodeId, decimal Amount, string? EffectiveDate, string? ExpiryDate, string FeeName)> feeConfigs,
        Guid contractId, string period, DateOnly dueDate);
}
