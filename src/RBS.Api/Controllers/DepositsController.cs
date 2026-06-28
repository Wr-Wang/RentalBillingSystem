using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepositsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] Guid? contractId) => Ok(new List<object>());

    [HttpPost("refund")]
    public IActionResult Refund([FromBody] object dto) => Ok(new { message = "退还成功" });

    [HttpPost("deduct")]
    public IActionResult Deduct([FromBody] object dto) => Ok(new { message = "扣除成功" });
}
