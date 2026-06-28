using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public RolesController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _uow.Roles.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Roles.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Organization.Role dto, CancellationToken ct)
    {
        await _uow.Roles.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Organization.Role dto, CancellationToken ct)
    {
        var entity = await _uow.Roles.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Roles.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.Roles.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpGet("{id}/menus")]
    public async Task<IActionResult> GetMenus(Guid id, CancellationToken ct)
    {
        var menus = await _uow.Menus.GetByRoleIdsAsync(new List<Guid> { id }, ct);
        return Ok(menus);
    }
}
