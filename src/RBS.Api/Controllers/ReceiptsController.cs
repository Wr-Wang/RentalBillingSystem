using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ReceiptsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, CancellationToken ct)
    {
        if (companyId == null) return Ok(new List<object>());
        var list = await _uow.Receipts.GetPendingConfirmAsync(companyId.Value, ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Billing.Receipt dto, CancellationToken ct)
    {
        await _uow.Receipts.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Receipts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.Confirm(Guid.Empty);
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] Dictionary<string, string> body, CancellationToken ct)
    {
        var entity = await _uow.Receipts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        body.TryGetValue("reason", out var reason);
        entity.Reject(reason ?? "驳回");
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPost("batch-confirm")]
    public async Task<IActionResult> BatchConfirm([FromBody] List<Guid> ids, CancellationToken ct)
    {
        return Ok(new { confirmed = ids.Count });
    }
}
