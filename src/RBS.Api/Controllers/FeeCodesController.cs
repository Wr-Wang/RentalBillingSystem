using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/fee-codes")]
[Authorize]
public class FeeCodesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public FeeCodesController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, CancellationToken ct)
    {
        if (companyId == null) return Ok(new List<object>());
        var list = await _uow.FeeCodes.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Billing.FeeCode dto, CancellationToken ct)
    {
        await _uow.FeeCodes.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Billing.FeeCode dto, CancellationToken ct)
    {
        var entity = await _uow.FeeCodes.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.FeeCodes.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.FeeCodes.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return NoContent();
    }
}
