using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public RoomsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? buildingId, CancellationToken ct)
    {
        if (buildingId == null) return Ok(new List<object>());
        var list = await _uow.Rooms.GetByBuildingIdAsync(buildingId.Value, ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Rooms.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Property.Room dto, CancellationToken ct)
    {
        await _uow.Rooms.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Property.Room dto, CancellationToken ct)
    {
        var entity = await _uow.Rooms.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Rooms.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.Rooms.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var buildings = await _uow.Buildings.GetAllAsync(ct);
        return Ok(buildings);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] List<RBS.Core.Entities.Property.Room> rooms, CancellationToken ct)
    {
        foreach (var r in rooms) await _uow.Rooms.AddAsync(r, ct);
        await _uow.CommitAsync(ct);
        return Ok(new { imported = rooms.Count });
    }
}
