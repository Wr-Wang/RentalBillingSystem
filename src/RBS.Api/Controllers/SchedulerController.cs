using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerService _service;
    public SchedulerController(ISchedulerService service) => _service = service;

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(CancellationToken ct) => Ok(await _service.GetJobsAsync(ct));

    [HttpPost("jobs")]
    public async Task<IActionResult> Create([FromBody] CreateJobScheduleRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("jobs/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobScheduleRequest request, CancellationToken ct)
    {
        await _service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
