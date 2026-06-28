using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LandlordsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public LandlordsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _uow.Landlords.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Landlords.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Organization.Landlord dto, CancellationToken ct)
    {
        await _uow.Landlords.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Organization.Landlord dto, CancellationToken ct)
    {
        var entity = await _uow.Landlords.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Landlords.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.Landlords.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpGet("{id}/stats")]
    public IActionResult GetStats(Guid id) => Ok(new { });
}
