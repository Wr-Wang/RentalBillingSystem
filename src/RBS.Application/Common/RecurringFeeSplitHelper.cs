using System.Data;
using Dapper;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.Common;

/// <summary>
/// 周期费用按月拆分辅助方法。
/// 封装 CalculateMonthlySplit + WithExpiry 批量插入的重复逻辑。
/// DD 说明：此工具类位于 Application 层是当前架构下的权宜方案，
/// 理想方案应抽取 IRecurringFeeSplitService 接口并由 Infrastructure 实现。
/// </summary>
public static class RecurringFeeSplitHelper
{
    /// <summary>
    /// 插入周期费用配置（按月拆分），返回所有配置 ID。
    /// 首段按天分摊，中间月全额，未来月全额 + 启用。
    /// </summary>
    public static async Task<List<Guid>> InsertMonthlySplitFeeConfigs(
        IDbConnection conn,
        IDbTransaction? tx,
        ISqlLoader sql,
        IBillingDomainService billingDomain,
        Guid contractId,
        Guid feeCodeId,
        decimal amount,
        string billingMode,
        string? unit,
        decimal? unitPrice,
        string effectiveDate,
        Guid createdBy,
        DateTime contractStartDate,
        DateTime? contractEndDate)
    {
        var segments = billingDomain.CalculateMonthlySplit(amount, effectiveDate, ChinaTime.Now,
            contractStartDate, contractEndDate);
        var ids = new List<Guid>();

        foreach (var seg in segments)
        {
            var segId = Guid.NewGuid();
            await conn.ExecuteAsync(
                sql.Get("Lease.Insert.ContractFeeConfig.WithExpiry"),
                new
                {
                    Id = segId,
                    ContractId = contractId,
                    FeeCodeId = feeCodeId,
                    BillingMode = billingMode,
                    Amount = seg.Amount,
                    Unit = unit,
                    UnitPrice = unitPrice,
                    IsActive = seg.IsActive,
                    EffectiveDate = seg.EffectiveDate,
                    ExpiryDate = seg.ExpiryDate,
                    CreatedBy = createdBy,
                    Now = ChinaTime.Now
                }, tx);
            ids.Add(segId);
        }

        return ids;
    }
}
