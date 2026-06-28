using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceivablesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ReceivablesController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        if (contractId == null) return Ok(new List<object>());
        var list = await _uow.ReceivablePlans.GetByContractIdAsync(contractId.Value, ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _uow.ReceivablePlans.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] object dto, CancellationToken ct)
    {
        return Ok(new { message = "生成任务已提交" });
    }
}
