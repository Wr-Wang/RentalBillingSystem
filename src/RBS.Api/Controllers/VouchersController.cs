using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Entities.Accounting;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VouchersController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ICurrentUserService _currentUser;

    public VouchersController(IDbConnectionFactory db, ISqlLoader sql, ICurrentUserService currentUser)
    {
        _db = db;
        _sql = sql;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var vouchers = await conn.QueryAsync<Voucher>(_sql.Get("Accounting.Select.Voucher.List"),
            new { StartDate = startDate, EndDate = endDate });
        return Ok(vouchers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Accounting.Select.Voucher.ByIdWithEntries"), new { Id = id });

        var voucher = await multi.ReadSingleOrDefaultAsync<Voucher>();
        if (voucher == null) return NotFound();

        var entries = (await multi.ReadAsync<JournalEntry>()).ToList();
        voucher.LoadEntries(entries);

        return Ok(voucher);
    }

    [HttpPut("{id}/post")]
    public async Task<IActionResult> Post(Guid id, CancellationToken ct)
    {
        using (var conn = _db.CreateConnection())
        {
            conn.Open();
            var entity = await conn.QuerySingleOrDefaultAsync<Voucher>(
                _sql.Get("Accounting.Select.Voucher.ById"), new { Id = id });
            if (entity == null) return NotFound();

            var entries = (await conn.QueryAsync<JournalEntry>(
                _sql.Get("Accounting.Select.Entry.ByVoucherId"), new { Id = id })).ToList();
            entity.LoadEntries(entries);

            entity.Post();

            var now = DateTime.UtcNow;
            var userId = _currentUser.UserId;
            await conn.ExecuteAsync(_sql.Get("Accounting.Update.Voucher.Post"),
                new { Status = entity.Status.Code, UpdatedBy = userId, UpdatedAt = now, Id = id });
        }
        return await Get(id, ct);
    }

    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] object dto, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var entity = await conn.QuerySingleOrDefaultAsync<Voucher>(
            _sql.Get("Accounting.Select.Voucher.ById"), new { Id = id });
        if (entity == null) return NotFound();

        var entries = (await conn.QueryAsync<JournalEntry>(
            _sql.Get("Accounting.Select.Entry.ByVoucherId"), new { Id = id })).ToList();
        entity.LoadEntries(entries);

        if (entity.Status.Code != "Posted")
            return BadRequest(new { message = "只能冲销已过账凭证" });

        entity.Unpost();
        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;
        await conn.ExecuteAsync(_sql.Get("Accounting.Update.Voucher.Post"),
            new { Status = entity.Status.Code, UpdatedBy = userId, UpdatedAt = now, Id = id });

        return Ok(new { message = "已冲销（反过账）", id });

    }
}
