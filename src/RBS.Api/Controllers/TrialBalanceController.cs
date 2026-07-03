using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrialBalanceController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromQuery] DateOnly? endDate, CancellationToken ct)
    {
        return Ok(new List<object>());
    }
}
