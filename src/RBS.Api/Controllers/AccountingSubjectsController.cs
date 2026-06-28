using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBS.Infrastructure.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountingSubjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AccountingSubjectsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, CancellationToken ct)
    {
        var query = _db.Set<RBS.Core.Entities.Accounting.AccountingSubject>().AsNoTracking();
        if (companyId != null) query = query.Where(s => s.CompanyId == companyId);
        return Ok(await query.ToListAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Accounting.AccountingSubject dto, CancellationToken ct)
    {
        _db.Add(dto);
        await _db.SaveChangesAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Accounting.AccountingSubject dto, CancellationToken ct)
    {
        var entity = await _db.FindAsync<RBS.Core.Entities.Accounting.AccountingSubject>(id, ct);
        if (entity == null) return NotFound();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.FindAsync<RBS.Core.Entities.Accounting.AccountingSubject>(id, ct);
        if (entity == null) return NotFound();
        _db.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
