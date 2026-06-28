using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBS.Infrastructure.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/journal-entries")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public JournalEntriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken ct)
    {
        var query = _db.Set<RBS.Core.Entities.Accounting.JournalEntry>().AsNoTracking();
        return Ok(await query.ToListAsync(ct));
    }
}
