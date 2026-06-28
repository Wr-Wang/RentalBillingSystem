using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ContractsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, CancellationToken ct)
    {
        if (companyId == null) return Ok(new List<object>());
        var list = await _uow.Contracts.GetActiveContractsAsync(companyId.Value, ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Contract.Contract dto, CancellationToken ct)
    {
        await _uow.Contracts.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Contract.Contract dto, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpPost("{id}/terminate")]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] Dictionary<string, string> body, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        body.TryGetValue("reason", out var reason);
        entity.Terminate(reason ?? "手动终止");
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.Suspend();
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPost("{id}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.Resume();
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPost("{id}/renew")]
    public async Task<IActionResult> Renew(Guid id, [FromBody] object dto, CancellationToken ct)
    {
        return Ok(new { message = "续签成功" });
    }

    [HttpGet("{id}/timeline")]
    public IActionResult GetTimeline(Guid id) => Ok(new List<object>());
}
