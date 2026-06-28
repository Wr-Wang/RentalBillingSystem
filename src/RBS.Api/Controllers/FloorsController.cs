using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FloorsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public FloorsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? buildingId, CancellationToken ct)
    {
        if (buildingId == null) return Ok(new List<object>());
        var rooms = await _uow.Rooms.GetByBuildingIdAsync(buildingId.Value, ct);
        return Ok(rooms);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] object dto, CancellationToken ct) => NoContent();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => NoContent();
}
