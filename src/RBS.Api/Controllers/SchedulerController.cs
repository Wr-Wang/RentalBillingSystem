using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/scheduler")]
[Authorize]
public class SchedulerController : ControllerBase
{
    [HttpGet("jobs")]
    public IActionResult GetJobs() => Ok(new List<object>());

    [HttpPost("jobs/{name}/trigger")]
    public IActionResult Trigger(string name) => Ok(new { message = $"任务 {name} 已触发" });
}
