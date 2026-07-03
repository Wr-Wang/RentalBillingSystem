using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollectionStagesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public CollectionStagesController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _uow.CollectionStages.GetAllAsync(ct);
        return Ok(list.OrderBy(s => s.DaysOverdue));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CollectionStage dto, CancellationToken ct)
    {
        await _uow.CollectionStages.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CollectionStage dto, CancellationToken ct)
    {
        var entity = await _uow.CollectionStages.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CollectionStages.UpdateAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.CollectionStages.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CollectionStages.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }
}
