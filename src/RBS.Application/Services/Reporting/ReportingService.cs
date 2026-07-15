using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Reporting;

/// <summary>报表服务 — 集中管理所有报表查询、聚合、富化逻辑</summary>
public class ReportingService : IReportingService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ReportingService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
    }

    public async Task<object> GetCollectionRateAsync(string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.CollectionRate"),
            new { Period = period });
    }

    public async Task<object> GetOverdueDetailAsync(Guid? companyId, string? period, CancellationToken ct)
    {
        if (!companyId.HasValue)
        {
            using var conn0 = _db.CreateConnection(); conn0.Open();
            var all = await conn0.QueryAsync(_sql.Get("Billing.Select.Journal.OverdueDetail"));
            var result0 = all.ToList();
            if (!string.IsNullOrEmpty(period)) result0 = result0.Where(p => (string)p.Period == period).ToList();
            return result0;
        }

        using var conn = _db.CreateConnection(); conn.Open();
        var raw = (await conn.QueryAsync<dynamic>(
            _sql.Get("Billing.Select.Journal.OverdueByCompany"),
            new { CompanyId = companyId.Value })).ToList();
        if (!string.IsNullOrEmpty(period)) raw = raw.Where(p => (string)p.Period == period).ToList();

        var ids = raw.Select(p => (Guid)p.ContractId).Distinct().ToList();
        var contracts = await conn.QueryAsync<(Guid, string, string, string)>(
            _sql.Get("Billing.Select.Contract.OverdueData"), new { Ids = ids });
        var contractDict = contracts.ToDictionary(c => c.Item1);

        var enriched = raw.Select(p =>
        {
            var info = contractDict.GetValueOrDefault((Guid)p.ContractId);
            return new
            {
                Id = (Guid)p.Id, ContractId = (Guid)p.ContractId, FeeCodeId = (Guid)p.FeeCodeId,
                Period = (string)p.Period, Amount = (decimal)p.Amount,
                DueDate = (DateOnly)p.DueDate, DaysOverdue = (int)p.DaysOverdue
            };
        }).OrderByDescending(p => p.DaysOverdue).ToList();

        return enriched;
    }

    public async Task<object> GetDailyReceiptAsync(DateOnly? date, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await conn.QueryAsync(
            _sql.Get("Billing.Select.Receipt.DailyReceipt"),
            new { D = d });
        return new { date = d, details = result };
    }

    public async Task<object> GetMonthlyReceiptAsync(string? period, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var p = period ?? $"{now.Year}-{now.Month:D2}";
        using var conn = _db.CreateConnection(); conn.Open();

        // 月度汇总
        var plans = await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.CollectionRate"),
            new { Period = p });

        // 每日收款明细（用于趋势图）
        var daily = await conn.QueryAsync(
            _sql.Get("Billing.Select.Receipt.DailyByMonth"),
            new { P = p });

        // 填充每日数据（无收款的日期补 0）
        var daysInMonth = DateTime.DaysInMonth(int.Parse(p.Split('-')[0]), int.Parse(p.Split('-')[1]));
        var dailyDict = daily.ToDictionary(d => (int)d.D, d => (decimal)d.Total);
        var dailyTotals = Enumerable.Range(1, daysInMonth).Select(d => dailyDict.GetValueOrDefault(d)).ToList();

        var summary = plans.FirstOrDefault();
        return new
        {
            period = p,
            totalAmount = summary?.TotalAmount ?? 0m,
            totalReceived = summary?.TotalReceived ?? 0m,
            dailyTotals
        };
    }

    public async Task<object> GetFeeRevenueAsync(string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var result = await conn.QueryAsync(
            _sql.Get("Billing.Select.FeeRevenue.All"),
            new { Period = period });
        return result;
    }

    public async Task<object> GetOccupancyRateAsync(string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var result = await conn.QueryAsync(
            _sql.Get("Billing.Select.HousingUnit.OccupancyRate"));
        return result;
    }
}
