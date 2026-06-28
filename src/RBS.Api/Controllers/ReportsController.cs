using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    [HttpGet("collection-rate")]
    public IActionResult GetCollectionRate([FromQuery] string? period) => Ok(new List<object>());

    [HttpGet("overdue-detail")]
    public IActionResult GetOverdueDetail([FromQuery] string? period) => Ok(new List<object>());

    [HttpGet("daily-receipt")]
    public IActionResult GetDailyReceipt([FromQuery] DateOnly? date) => Ok(new List<object>());

    [HttpGet("monthly-receipt")]
    public IActionResult GetMonthlyReceipt([FromQuery] string? period) => Ok(new List<object>());

    [HttpGet("fee-revenue")]
    public IActionResult GetFeeRevenue([FromQuery] string? period) => Ok(new List<object>());

    [HttpGet("occupancy-rate")]
    public IActionResult GetOccupancyRate([FromQuery] string? period) => Ok(new List<object>());
}
