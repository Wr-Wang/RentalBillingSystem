using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Property;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FloorLevelBandsController : ControllerBase
{
    private readonly IFloorLevelBandService _service;
    public FloorLevelBandsController(IFloorLevelBandService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await _service.GetListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFloorLevelBandRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFloorLevelBandRequest request, CancellationToken ct)
    {
        await _service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
