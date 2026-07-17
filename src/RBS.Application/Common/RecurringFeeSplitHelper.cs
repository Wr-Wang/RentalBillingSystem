using System.Data;
using Dapper;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.Common;

/// <summary>
/// 周期费用按月拆分辅助方法。
/// 封装 CalculateMonthlySplit + WithExpiry 批量插入 + 首月分摊 Journal 生成的重复逻辑。
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
        Guid createdBy)
    {
        var segments = billingDomain.CalculateMonthlySplit(amount, effectiveDate, ChinaTime.Now);
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

    /// <summary>
    /// 周期费用拆分配置后，生成首月分摊应收 Journal（GLPosted=0）。
    /// 如果首段没有到期日（未来配置），则不生成。
    /// </summary>
    public static async Task GenerateFirstMonthJournal(
        IDbConnection conn,
        IDbTransaction? tx,
        ISqlLoader sql,
        List<FeeMonthSegment> segments,
        List<Guid> configIds,
        Guid companyId,
        Guid contractId,
        Guid feeCodeId,
        string feeName)
    {
        var firstSeg = segments[0];
        if (firstSeg.ExpiryDate == null) return;

        var segEffDate = DateOnly.Parse(firstSeg.EffectiveDate);
        var period = segEffDate.ToString("yyyy-MM");

        await conn.ExecuteAsync(
            sql.Get("Billing.Insert.Journal.Unposted"),
            new
            {
                Id = Guid.NewGuid(),
                CoId = companyId,
                CId = contractId,
                FId = feeCodeId,
                FConfigId = configIds[0],
                SubjId = Guid.Empty,
                Period = period,
                Amt = firstSeg.Amount,
                Due = DateOnly.FromDateTime(ChinaTime.Now),
                EntryType = "Normal",
                BilledAt = ChinaTime.Now,
                DNId = (Guid?)null,
                ParentId = (Guid?)null,
                Summary = $"应收 {feeName} {period}",
                CBy = Guid.Empty
            }, tx);
    }
}
