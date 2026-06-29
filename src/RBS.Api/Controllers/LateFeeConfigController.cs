using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LateFeeConfigController : ControllerBase
{
    private readonly ILateFeeConfigService _service;
    public LateFeeConfigController(ILateFeeConfigService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct)
        => Ok(await _service.GetListAsync(ct));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
        => Ok(await _service.GetActiveAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveLateFeeConfigRequest request, CancellationToken ct)
        => Ok(await _service.SaveAsync(request, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
