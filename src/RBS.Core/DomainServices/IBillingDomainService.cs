namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.SystemConfig;

/// <summary>
/// 计费领域服务接口 — 应收计划生成和滞纳金计算
/// </summary>
public interface IBillingDomainService
{
    /// <summary>为合同批量生成指定账期的应收计划</summary>
    List<ReceivablePlan> GenerateReceivablePlans(Contract contract, string period, DateOnly dueDate);

    /// <summary>计算滞纳金</summary>
    decimal CalculateLateFee(ReceivablePlan plan, LateFeeConfig config, DateOnly asOfDate);

    /// <summary>按天分摊金额</summary>
    decimal CalculateProratedAmount(decimal monthlyAmount, int daysInPeriod, int occupiedDays);
}
