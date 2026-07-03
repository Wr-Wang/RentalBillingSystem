using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    public ReportsController(IUnitOfWork uow, IDbConnectionFactory db) { _uow = uow; _db = db; }

    /// <summary>收款率 — 指定账期的应收/已收/收款率</summary>
    [HttpGet("collectionrate")]
    public async Task<IActionResult> GetCollectionRate([FromQuery] string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var sql = @"SELECT
            p.Period, COUNT(1) AS TotalPlans,
            SUM(p.Amount) AS TotalAmount, SUM(p.Received) AS TotalReceived,
            CASE WHEN SUM(p.Amount) > 0 THEN ROUND(SUM(p.Received)/SUM(p.Amount)*100,1) ELSE 0 END AS Rate
        FROM ReceivablePlans p
        WHERE (@Period IS NULL OR p.Period = @Period)
        GROUP BY p.Period ORDER BY p.Period DESC";
        var result = await conn.QueryAsync(sql, new { Period = period });
        return Ok(result);
    }

    /// <summary>逾期明细 — 当前逾期应收明细</summary>
    [HttpGet("overduedetail")]
    public async Task<IActionResult> GetOverdueDetail([FromQuery] Guid? companyId, [FromQuery] string? period, CancellationToken ct)
    {
        if (!companyId.HasValue)
        {
            var all = await _uow.ReceivablePlans.GetAllAsync(ct);
            var overdue = all.Where(p => p.IsOverdue);
            if (!string.IsNullOrEmpty(period)) overdue = overdue.Where(p => p.Period == period);
            return Ok(overdue.OrderByDescending(p => p.DaysOverdue));
        }

        // 有 companyId 时使用仓储的逾期查询（走 SQL 过滤）
        var plans = await _uow.ReceivablePlans.GetOverdueAsync(companyId.Value, ct);
        if (!string.IsNullOrEmpty(period)) plans = plans.Where(p => p.Period == period).ToList();
        return Ok(plans.OrderByDescending(p => p.DaysOverdue));
    }

    /// <summary>日报 — 指定日期的收款汇总</summary>
    [HttpGet("dailyreceipt")]
    public async Task<IActionResult> GetDailyReceipt([FromQuery] DateOnly? date, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await conn.QueryAsync(
            "SELECT Status, COUNT(1) AS Cnt, SUM(Amount) AS Total FROM Receipts WHERE ReceivedDate=@D GROUP BY Status",
            new { D = d });
        return Ok(new { date = d, details = result });
    }

    /// <summary>月报 — 指定账期的收款、应收、逾期统计</summary>
    [HttpGet("monthlyreceipt")]
    public async Task<IActionResult> GetMonthlyReceipt([FromQuery] string? period, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var p = period ?? $"{now.Year}-{now.Month:D2}";
        using var conn = _db.CreateConnection(); conn.Open();
        var plans = await conn.QueryAsync(
            "SELECT COUNT(1) AS TotalPlans, SUM(Amount) AS TotalAmount, SUM(Received) AS TotalReceived FROM ReceivablePlans WHERE Period=@P",
            new { P = p });
        var receipts = await conn.QueryAsync(
            "SELECT COUNT(1) AS Cnt, SUM(Amount) AS Total FROM Receipts WHERE FORMAT(ReceivedDate, 'yyyy-MM')=@P",
            new { P = p });
        return Ok(new { period = p, plans = plans.FirstOrDefault(), receipts = receipts.FirstOrDefault() });
    }

    /// <summary>费用收入 — 指定账期按费用类型汇总应收</summary>
    [HttpGet("feerevenue")]
    public async Task<IActionResult> GetFeeRevenue([FromQuery] string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var result = await conn.QueryAsync(
            @"SELECT f.Name AS FeeName, f.Code, COUNT(1) AS Cnt, SUM(p.Amount) AS TotalAmount, SUM(p.Received) AS TotalReceived
            FROM ReceivablePlans p INNER JOIN FeeCodes f ON f.Id = p.FeeCodeId
            WHERE (@Period IS NULL OR p.Period = @Period)
            GROUP BY f.Name, f.Code ORDER BY TotalAmount DESC",
            new { Period = period });
        return Ok(result);
    }

    /// <summary>出租率 — 按楼栋统计房间出租率</summary>
    [HttpGet("occupancyrate")]
    public async Task<IActionResult> GetOccupancyRate([FromQuery] string? period, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var result = await conn.QueryAsync(
            @"SELECT b.Name AS BuildingName, b.Id AS BuildingId,
            COUNT(r.Id) AS TotalRooms,
            SUM(CASE WHEN r.Status='Rented' THEN 1 ELSE 0 END) AS RentedRooms,
            ROUND(CAST(SUM(CASE WHEN r.Status='Rented' THEN 1 ELSE 0 END) AS FLOAT)/NULLIF(COUNT(r.Id),0)*100,1) AS OccupancyRate
            FROM Buildings b LEFT JOIN Rooms r ON r.BuildingId = b.Id
            GROUP BY b.Name, b.Id ORDER BY b.Name");
        return Ok(result);
    }
}
