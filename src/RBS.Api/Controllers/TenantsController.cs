using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public TenantsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(keyword))
        {
            var result = await _uow.Tenants.SearchAsync(keyword, ct);
            return Ok(result);
        }
        var list = await _uow.Tenants.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Tenants.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Contract.Tenant dto, CancellationToken ct)
    {
        await _uow.Tenants.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Contract.Tenant dto, CancellationToken ct)
    {
        var entity = await _uow.Tenants.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Tenants.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.Tenants.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }
}
