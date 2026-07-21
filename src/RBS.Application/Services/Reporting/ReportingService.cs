using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Reporting;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Common;
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

    public async Task<object> GetCollectionRateAsync(Guid? companyId, string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.CollectionRate"),
            new { CompanyId = companyId, Period = period });
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
                DueDate = DateOnly.FromDateTime((DateTime)p.DueDate), DaysOverdue = (int)p.DaysOverdue
            };
        }).OrderByDescending(p => p.DaysOverdue).ToList();

        return enriched;
    }

    public async Task<object> GetDailyReceiptAsync(Guid? companyId, DateOnly? date, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var d = date ?? DateOnly.FromDateTime(ChinaTime.Now);
        var result = await conn.QueryAsync(
            _sql.Get("Billing.Select.Receipt.DailyReceipt"),
            new { D = d, CompanyId = companyId });
        return new { date = d, details = result };
    }

    public async Task<object> GetMonthlyReceiptAsync(Guid? companyId, string? period, CancellationToken ct)
    {
        var now = ChinaTime.Now;
        var p = period ?? $"{now.Year}-{now.Month:D2}";
        using var conn = _db.CreateConnection(); conn.Open();

        // 月度汇总
        var plans = await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.CollectionRate"),
            new { CompanyId = companyId, Period = p });

        // 每日收款明细（用于趋势图）
        var daily = await conn.QueryAsync(
            _sql.Get("Billing.Select.Receipt.DailyByMonth"),
            new { P = p, CompanyId = companyId });

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
            new { Period = period, CompanyId = (Guid?)null });
        return result;
    }

    public async Task<object> GetOccupancyRateAsync(string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var result = await conn.QueryAsync(
            _sql.Get("Billing.Select.HousingUnit.OccupancyRate"));
        return result;
    }

    public async Task<MultiCompanyOverviewDto> GetMultiCompanyOverviewAsync(string? period, CancellationToken ct)
    {
        var now = ChinaTime.Now;
        var p = period ?? $"{now.Year}-{now.Month:D2}";

        // 计算上月账期
        var prevMonth = now.AddMonths(-1);
        var prevPeriod = $"{prevMonth.Year}-{prevMonth.Month:D2}";

        using var conn = _db.CreateConnection(); conn.Open();

        var rows = await conn.QueryAsync<MultiCompanyOverviewRow>(
            _sql.Get("Billing.Select.Report.MultiCompanyOverview"),
            new { Period = p, PrevPeriod = prevPeriod });

        var items = rows.Select(r => new CompanyOverviewItem
        {
            Id = r.Id,
            Name = r.Name,
            IsActive = r.IsActive,
            BuildingCount = r.BuildingCount,
            RoomCount = r.RoomCount,
            RentedCount = r.RentedCount,
            OccupancyRate = r.OccupancyRate,
            MonthlyReceivable = r.MonthlyReceivable,
            MonthlyReceived = r.MonthlyReceived,
            CollectionRate = r.CollectionRate,
            OverdueAmount = r.OverdueAmount,
            OverdueCount = r.OverdueCount,
            ActiveContractCount = r.ActiveContractCount,
            TotalContractCount = r.TotalContractCount,
            PrevMonthCollectionRate = r.PrevMonthCollectionRate > 0 ? r.PrevMonthCollectionRate : null
        }).ToList();

        var activeItems = items.Where(i => i.IsActive).ToList();

        var totalBuilding = activeItems.Sum(i => i.BuildingCount);
        var totalRoom = activeItems.Sum(i => i.RoomCount);
        var totalRented = activeItems.Sum(i => i.RentedCount);
        var totalReceivable = activeItems.Sum(i => i.MonthlyReceivable);
        var totalReceived = activeItems.Sum(i => i.MonthlyReceived);
        var totalOverdue = activeItems.Sum(i => i.OverdueAmount);
        var totalOverdueCount = activeItems.Sum(i => i.OverdueCount);
        var totalActiveContracts = activeItems.Sum(i => i.ActiveContractCount);

        var avgOccupancy = activeItems.Count > 0
            ? Math.Round(activeItems.Average(i => i.OccupancyRate), 2)
            : 0m;
        var avgCollection = totalReceivable > 0
            ? Math.Round(totalReceived / totalReceivable * 100, 2)
            : 0m;

        // 环比
        decimal? prevCollectionRate = null;
        if (activeItems.Any(i => i.PrevMonthCollectionRate.HasValue))
        {
            var prevCollectionRates = activeItems
                .Where(i => i.PrevMonthCollectionRate.HasValue && i.MonthlyReceivable > 0)
                .Select(i => i.PrevMonthCollectionRate!.Value);
            if (prevCollectionRates.Any())
                prevCollectionRate = Math.Round(prevCollectionRates.Average(), 2);
        }

        return new MultiCompanyOverviewDto
        {
            Period = p,
            TotalCompanies = items.Count,
            ActiveCompanies = activeItems.Count,
            TotalBuildings = totalBuilding,
            TotalRooms = totalRoom,
            TotalRented = totalRented,
            AvgOccupancyRate = avgOccupancy,
            TotalMonthlyReceivable = totalReceivable,
            TotalMonthlyReceived = totalReceived,
            AvgCollectionRate = avgCollection,
            TotalOverdueAmount = totalOverdue,
            TotalOverdueCount = totalOverdueCount,
            TotalActiveContracts = totalActiveContracts,
            CollectionRateChange = null,      // 同比需要去年数据
            CollectionRateMomChange = prevCollectionRate.HasValue && prevCollectionRate > 0
                ? Math.Round(avgCollection - prevCollectionRate.Value, 2)
                : null,
            OccupancyRateChange = null,
            Companies = items
        };
    }

    /// <summary>多公司总览查询中间行（Dapper 映射用）</summary>
    private class MultiCompanyOverviewRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int BuildingCount { get; set; }
        public int RoomCount { get; set; }
        public int RentedCount { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal MonthlyReceivable { get; set; }
        public decimal MonthlyReceived { get; set; }
        public decimal CollectionRate { get; set; }
        public decimal OverdueAmount { get; set; }
        public int OverdueCount { get; set; }
        public int ActiveContractCount { get; set; }
        public int TotalContractCount { get; set; }
        public decimal PrevMonthCollectionRate { get; set; }
    }
}
