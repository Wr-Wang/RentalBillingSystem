using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reporting;

    public ReportsController(IReportingService reporting) { _reporting = reporting; }

    [HttpGet("collectionrate")]
    public async Task<IActionResult> GetCollectionRate([FromQuery] Guid? companyId, [FromQuery] string? period, CancellationToken ct)
        => Ok(await _reporting.GetCollectionRateAsync(companyId, period, ct));

    [HttpGet("overduedetail")]
    public async Task<IActionResult> GetOverdueDetail([FromQuery] Guid? companyId, [FromQuery] string? period, CancellationToken ct)
        => Ok(await _reporting.GetOverdueDetailAsync(companyId, period, ct));

    [HttpGet("dailyreceipt")]
    public async Task<IActionResult> GetDailyReceipt([FromQuery] Guid? companyId, [FromQuery] DateOnly? date, CancellationToken ct)
        => Ok(await _reporting.GetDailyReceiptAsync(companyId, date, ct));

    [HttpGet("monthlyreceipt")]
    public async Task<IActionResult> GetMonthlyReceipt([FromQuery] Guid? companyId, [FromQuery] string? period, CancellationToken ct)
        => Ok(await _reporting.GetMonthlyReceiptAsync(companyId, period, ct));

    [HttpGet("feerevenue")]
    public async Task<IActionResult> GetFeeRevenue([FromQuery] string? period, CancellationToken ct)
        => Ok(await _reporting.GetFeeRevenueAsync(period, ct));

    [HttpGet("occupancyrate")]
    public async Task<IActionResult> GetOccupancyRate([FromQuery] string? period, CancellationToken ct)
        => Ok(await _reporting.GetOccupancyRateAsync(period, ct));
}
