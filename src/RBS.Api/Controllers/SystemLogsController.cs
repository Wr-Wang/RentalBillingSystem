using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SystemLogsController(AppDbContext db) => _db = db;

    private bool IsSuperAdmin => User.FindFirst("IsSuperAdmin")?.Value == "True";

    private IActionResult? RequireSuperAdmin()
    {
        if (!IsSuperAdmin) return Forbid();
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? level = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var query = _db.SystemLogs.AsNoTracking();
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);
        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new
        {
            items = items.Select(l => new
            {
                l.Id, l.Level, l.Message, l.Source, l.Path, l.Method,
                l.IpAddress, l.UserId, l.UserDisplayName, l.CreatedAt
            }),
            total,
            page,
            pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var log = await _db.SystemLogs.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);
        if (log == null) return NotFound();
        return Ok(log);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var log = await _db.SystemLogs.FindAsync(new object[] { id }, ct);
        if (log == null) return NotFound();
        _db.SystemLogs.Remove(log);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearAll(CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        _db.SystemLogs.RemoveRange(await _db.SystemLogs.ToListAsync(ct));
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
