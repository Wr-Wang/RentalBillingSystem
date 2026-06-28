using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeterReadingsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public MeterReadingsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractFeeConfigId, CancellationToken ct)
    {
        if (contractFeeConfigId == null) return Ok(new List<object>());
        var list = await _uow.MeterReadings.GetHistoryAsync(contractFeeConfigId.Value, RBS.Core.Common.ChinaTime.Now.Year, RBS.Core.Common.ChinaTime.Now.Month, ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Billing.MeterReading dto, CancellationToken ct)
    {
        await _uow.MeterReadings.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Billing.MeterReading dto, CancellationToken ct)
    {
        var entity = await _uow.MeterReadings.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var entity = await _uow.MeterReadings.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] List<RBS.Core.Entities.Billing.MeterReading> list, CancellationToken ct)
    {
        return Ok(new { imported = 0 });
    }
}
