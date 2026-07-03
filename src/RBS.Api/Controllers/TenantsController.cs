using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Entities.Contract;
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
    public async Task<IActionResult> Create([FromBody] TenantRequest request, CancellationToken ct)
    {
        if (request.CompanyId == Guid.Empty)
            return BadRequest(new { message = "companyId 不能为空" });
        var entity = new Tenant(request.Name, request.CompanyId);
        if (!string.IsNullOrEmpty(request.Phone)) entity.SetPhone(request.Phone);
        if (!string.IsNullOrEmpty(request.IdCard)) entity.SetIdCard(request.IdCard);
        await _uow.Tenants.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TenantRequest request, CancellationToken ct)
    {
        var entity = await _uow.Tenants.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        if (request.Name != null) entity.Rename(request.Name);
        if (request.Phone != null) entity.SetPhone(request.Phone);
        if (request.IdCard != null) entity.SetIdCard(request.IdCard);
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

public class TenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdCard { get; set; }
    public Guid CompanyId { get; set; }
}
