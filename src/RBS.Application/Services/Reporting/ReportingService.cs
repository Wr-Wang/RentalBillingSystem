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
        var sql = @"SELECT
            p.Period, COUNT(1) AS TotalPlans,
            SUM(p.Amount) AS TotalAmount, SUM(p.Received) AS TotalReceived,
            CASE WHEN SUM(p.Amount) > 0 THEN ROUND(SUM(p.Received)/SUM(p.Amount)*100,1) ELSE 0 END AS Rate
        FROM ReceivablePlans p
        WHERE (@Period IS NULL OR p.Period = @Period)
        GROUP BY p.Period ORDER BY p.Period DESC";
        return await conn.QueryAsync(sql, new { Period = period });
    }

    public async Task<object> GetOverdueDetailAsync(Guid? companyId, string? period, CancellationToken ct)
    {
        if (!companyId.HasValue)
        {
            using var conn0 = _db.CreateConnection(); conn0.Open();
            var all = await conn0.QueryAsync(_sql.Get("Receivable.Select.Plan.OverdueDetail"));
            var result0 = all.ToList();
            if (!string.IsNullOrEmpty(period)) result0 = result0.Where(p => (string)p.Period == period).ToList();
            return result0;
        }

        using var conn = _db.CreateConnection(); conn.Open();
        var raw = await _uow.ReceivablePlans.GetOverdueAsync(companyId.Value, ct);
        if (!string.IsNullOrEmpty(period)) raw = raw.Where(p => p.Period == period).ToList();

        var ids = raw.Select(p => p.ContractId).Distinct().ToList();
        var contracts = await conn.QueryAsync<(Guid, string, string, string)>(
            @"SELECT c.Id, c.ContractNo, t.Name AS TenantName, h.FullCode AS RoomFullCode
              FROM Contracts c
              LEFT JOIN ContractTenants ct ON ct.ContractId = c.Id AND ct.IsPrimary = 1
              LEFT JOIN Tenants t ON t.Id = ct.TenantId
              LEFT JOIN HousingUnits h ON h.Id = c.RoomId
              WHERE c.Id IN @Ids", new { Ids = ids });
        var contractDict = contracts.ToDictionary(c => c.Item1);

        var enriched = raw.Select(p =>
        {
            var info = contractDict.GetValueOrDefault(p.ContractId);
            return new
            {
                p.Id, p.ContractId, p.FeeCodeId, p.Period, p.Amount, p.Received,
                p.DueDate, p.Status, p.LateFee, p.DaysOverdue, p.IsOverdue,
                ContractNo = info.Item2 ?? "",
                TenantName = info.Item3 ?? "",
                RoomFullCode = info.Item4 ?? ""
            };
        }).OrderByDescending(p => p.DaysOverdue).ToList();

        return enriched;
    }

    public async Task<object> GetDailyReceiptAsync(DateOnly? date, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await conn.QueryAsync(
            "SELECT Status, COUNT(1) AS Cnt, SUM(Amount) AS Total FROM Receipts WHERE ReceivedDate=@D GROUP BY Status",
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
            "SELECT COUNT(1) AS TotalPlans, SUM(Amount) AS TotalAmount, SUM(Received) AS TotalReceived FROM ReceivablePlans WHERE Period=@P",
            new { P = p });

        // 每日收款明细（用于趋势图）
        var daily = await conn.QueryAsync(
            "SELECT DAY(ReceivedDate) AS D, COUNT(1) AS Cnt, SUM(Amount) AS Total FROM Receipts WHERE FORMAT(ReceivedDate, 'yyyy-MM')=@P GROUP BY DAY(ReceivedDate) ORDER BY D",
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
            @"SELECT f.Name AS FeeName, f.Code, COUNT(1) AS Cnt, SUM(p.Amount) AS TotalAmount, SUM(p.Received) AS TotalReceived
            FROM ReceivablePlans p INNER JOIN FeeCodes f ON f.Id = p.FeeCodeId
            WHERE (@Period IS NULL OR p.Period = @Period)
            GROUP BY f.Name, f.Code ORDER BY TotalAmount DESC",
            new { Period = period });
        return result;
    }

    public async Task<object> GetOccupancyRateAsync(string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var result = await conn.QueryAsync(
            @"SELECT hu.BuildingName, hu.Id AS BuildingId,
            COUNT(r.Id) AS TotalRooms,
            SUM(CASE WHEN r.Status='Rented' THEN 1 ELSE 0 END) AS RentedRooms,
            ROUND(CAST(SUM(CASE WHEN r.Status='Rented' THEN 1 ELSE 0 END) AS FLOAT)/NULLIF(COUNT(r.Id),0)*100,1) AS OccupancyRate
            FROM HousingUnits hu
            LEFT JOIN Rooms r ON r.BuildingId = hu.Id
            WHERE hu.BuildingName IS NOT NULL
            GROUP BY hu.BuildingName, hu.Id ORDER BY hu.BuildingName");
        return result;
    }
}
