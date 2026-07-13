using System.Data;
using System.Linq;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.DomainServices;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 日记账生成服务 — 按 FeeConfig 预生成 Voucher + JournalEntry
/// </summary>
public class JournalGenerationService : IJournalGenerationService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IBillingDomainService _billingDomain;

    public JournalGenerationService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql, IBillingDomainService billingDomain)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
        _billingDomain = billingDomain;
    }

    /// <summary>
    /// 生成 OneTime 费用的 JE（合同签署时调用，如押金）
    /// ★ 幂等：已存在同一 FeeConfig 的 Voucher 则跳过
    /// </summary>
    public async Task GenerateOneTimeAsync(Guid contractId, Guid feeConfigId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        // ★ 幂等检查：该 FeeConfig 是否已生成过 Voucher
        var exists = await conn.QuerySingleAsync<int>(
            _sql.Get("Accounting.Select.Voucher.ExistsByFeeConfigId"),
            new { FeeConfigId = feeConfigId });
        if (exists > 0) return;

        // 获取 FeeConfig + FeeCode
        var config = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Journal.Select.OneTimeFeeConfig.WithFeeCode"),
            new { Id = feeConfigId, Cid = contractId });

        if (config == null) return;
        if (config.ChargeType != "OneTime") return; // 只处理一次性费用

        // 查会计科目
        var subjects = await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes"));
        var subjectMap = subjects.ToDictionary(x => x.Code, x => x.Id);
        var receivableId = subjectMap.GetValueOrDefault("1122", Guid.Empty);
        var depositArId = subjectMap.GetValueOrDefault("112202", Guid.Empty);
        var depositLiabilityId = subjectMap.GetValueOrDefault("2241", Guid.Empty);
        var revenueId = subjectMap.GetValueOrDefault("6001", subjectMap.GetValueOrDefault("6051", Guid.Empty));

        var now = ChinaTime.Now;
        var period = DateOnly.FromDateTime(now).ToString("yyyy-MM");
        var voucherId = Guid.NewGuid();
        var voucherNo = $"OT-{now:yyyyMMdd}-{voucherId:N}".Truncate(32);

        // 插入 Voucher（含 Period），关联 FeeConfigId 用于幂等去重
        await conn.ExecuteAsync(
            _sql.Get("Accounting.Insert.Voucher.WithPeriodAndFeeConfig"),
            new
            {
                Id = voucherId,
                No = voucherNo,
                Date = DateOnly.FromDateTime(now),
                SrcId = contractId,
                Type = "ContractFee.Immediate",
                CId = contractId,
                FConfigId = feeConfigId,
                Period = period,
                CBy = Guid.Empty
            });

        // 押金：DEBIT 112202（应收押金）/ CREDIT 2241（其他应付款-押金）
        if (config.Code == "DEPOSIT" && depositArId != Guid.Empty && depositLiabilityId != Guid.Empty)
        {
            await conn.ExecuteAsync(
                _sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = voucherId, SId = depositArId, Dir = "Debit", Amt = (decimal)config.Amount, Sum = $"{config.FeeName}", CBy = Guid.Empty });
            await conn.ExecuteAsync(
                _sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = voucherId, SId = depositLiabilityId, Dir = "Credit", Amt = (decimal)config.Amount, Sum = $"{config.FeeName}", CBy = Guid.Empty });
        }
        else if (config.ChargeType == "OneTime" && receivableId != Guid.Empty && revenueId != Guid.Empty)
        {
            // 其他一次性费用：DEBIT 1122（应收）/ CREDIT 6001(6051)（收入）
            await conn.ExecuteAsync(
                _sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = voucherId, SId = receivableId, Dir = "Debit", Amt = (decimal)config.Amount, Sum = $"{config.FeeName}（一次性）", CBy = Guid.Empty });
            await conn.ExecuteAsync(
                _sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = voucherId, SId = revenueId, Dir = "Credit", Amt = (decimal)config.Amount, Sum = $"{config.FeeName}（一次性）", CBy = Guid.Empty });
        }
    }

    /// <summary>
    /// 生成补差 Supplementary JE（费用调价账单已出时调用，独立连接）
    /// </summary>
    public async Task GenerateSupplementaryAsync(Guid contractId, Guid feeCodeId,
        decimal newAmount, decimal oldAmount, string effectiveDate, string period, CancellationToken ct)
    {
        // ★ 先查会计科目（只读字典，不需要事务）
        Dictionary<string, Guid> subjectMap;
        using (var subConn = _db.CreateConnection())
        {
            subConn.Open();
            var subjects = await subConn.QueryAsync<(string Code, Guid Id)>(
                _sql.Get("Accounting.Select.Subject.ByCodes"));
            subjectMap = subjects.ToDictionary(x => x.Code, x => x.Id);
        }

        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await GenerateSupplementaryCoreAsync(conn, tx, subjectMap, contractId, feeCodeId, newAmount, oldAmount, effectiveDate, period, ct);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 生成补差 Supplementary JE（事务内重载，与调用方共享连接）
    /// </summary>
    public async Task GenerateSupplementaryAsync(IDbConnection conn, IDbTransaction tx,
        Guid contractId, Guid feeCodeId, decimal newAmount, decimal oldAmount, string effectiveDate, string period, CancellationToken ct)
    {
        // ★ 调用方已加载会计科目，此处不再重复查询
        var subjectMap = await LoadSubjectMapAsync(conn, tx, ct);
        await GenerateSupplementaryCoreAsync(conn, tx, subjectMap, contractId, feeCodeId, newAmount, oldAmount, effectiveDate, period, ct);
    }

    /// <summary>加载会计科目映射</summary>
    private async Task<Dictionary<string, Guid>> LoadSubjectMapAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct)
    {
        var subjects = await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes"), transaction: tx);
        return subjects.ToDictionary(x => x.Code, x => x.Id);
    }

    /// <summary>核心逻辑：计算差价并插入 Voucher + JournalEntry（共享连接/事务）</summary>
    private async Task GenerateSupplementaryCoreAsync(IDbConnection conn, IDbTransaction tx,
        Dictionary<string, Guid> subjectMap, Guid contractId, Guid feeCodeId,
        decimal newAmount, decimal oldAmount, string effectiveDate, string period, CancellationToken ct)
    {
        var receivableId = subjectMap.GetValueOrDefault("1122", Guid.Empty);
        var revenueId = subjectMap.GetValueOrDefault("6001", subjectMap.GetValueOrDefault("6051", Guid.Empty));

        if (receivableId == Guid.Empty || revenueId == Guid.Empty) return;

        // 获取 FeeConfig 信息
        var config = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Journal.Select.SupplementaryFeeConfig.ByContractAndFee"),
            new { Cid = contractId, Fid = feeCodeId }, tx);

        if (config == null) return;

        // 判断是否需要分摊
        decimal diffAmount;
        var currentMonth = DateOnly.FromDateTime(ChinaTime.Now).ToString("yyyy-MM");

        if (period == currentMonth)
        {
            // 当月：分摊计算
            var monthStart = DateOnly.Parse($"{period}-01");
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var effDate = DateOnly.Parse(effectiveDate);
            var coveredDays = daysInMonth - effDate.Day + 1;

            var oldPortion = oldAmount / daysInMonth * (effDate.Day - 1);
            var newPortion = newAmount / daysInMonth * coveredDays;
            diffAmount = Math.Round(oldPortion + newPortion - oldAmount, 2);
        }
        else
        {
            // 整月全额差价
            diffAmount = newAmount - oldAmount;
        }

        if (diffAmount == 0) return;

        var now = ChinaTime.Now;
        var voucherId = Guid.NewGuid();
        var voucherNo = $"SUP-{now:yyyyMMdd}-{voucherId:N}".Truncate(32);

        // 插入 Voucher（含 Period）
        await conn.ExecuteAsync(
            _sql.Get("Accounting.Insert.Voucher.WithPeriod"),
            new
            {
                Id = voucherId,
                No = voucherNo,
                Date = DateOnly.FromDateTime(now),
                Desc = $"{config.FeeName}调价补差（{period}）",
                SrcId = contractId,
                Type = "ContractFee.Supplementary",
                CId = contractId,
                Period = period,
                CBy = Guid.Empty
            }, tx);

        // DEBIT 1122 / CREDIT 6001(或6051)
        await conn.ExecuteAsync(
            _sql.Get("Accounting.Insert.JournalEntry.Simple"),
            new { Id = Guid.NewGuid(), VId = voucherId, SId = receivableId, Dir = "Debit", Amt = diffAmount, Sum = $"{config.FeeName}补差", CBy = Guid.Empty }, tx);
        await conn.ExecuteAsync(
            _sql.Get("Accounting.Insert.JournalEntry.Simple"),
            new { Id = Guid.NewGuid(), VId = voucherId, SId = revenueId, Dir = "Credit", Amt = diffAmount, Sum = $"{config.FeeName}补差", CBy = Guid.Empty }, tx);
    }
}

/// <summary>字符串扩展</summary>
internal static class StringExtensions
{
    public static string Truncate(this string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
