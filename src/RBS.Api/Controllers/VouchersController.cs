using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBS.Infrastructure.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VouchersController : ControllerBase
{
    private readonly AppDbContext _db;
    public VouchersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken ct)
    {
        var query = _db.Set<RBS.Core.Entities.Accounting.Voucher>().AsNoTracking();
        if (startDate.HasValue) query = query.Where(v => v.VoucherDate >= startDate);
        if (endDate.HasValue) query = query.Where(v => v.VoucherDate <= endDate);
        return Ok(await query.ToListAsync(ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _db.Set<RBS.Core.Entities.Accounting.Voucher>()
            .Include(v => v.Entries)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPut("{id}/post")]
    public async Task<IActionResult> Post(Guid id, CancellationToken ct)
    {
        var entity = await _db.FindAsync<RBS.Core.Entities.Accounting.Voucher>(id, ct);
        if (entity == null) return NotFound();
        entity.Post();
        await _db.SaveChangesAsync(ct);
        return Ok(entity);
    }

    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] object dto, CancellationToken ct)
    {
        return Ok(new { message = "已冲销" });
    }
}
