using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenusController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public MenusController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _uow.Menus.GetTreeAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Organization.Menu dto, CancellationToken ct)
    {
        await _uow.Menus.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Organization.Menu dto, CancellationToken ct)
    {
        var entity = await _uow.Menus.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Menus.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.Menus.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }
}
