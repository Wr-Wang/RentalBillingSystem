using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceivablesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IReceivableGenerationService _generationService;

    public ReceivablesController(IUnitOfWork uow, IReceivableGenerationService generationService)
    {
        _uow = uow;
        _generationService = generationService;
    }

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
    public async Task<IActionResult> Generate(
        [FromBody] GenerateReceivablesRequest request,
        CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty)
            return BadRequest(new { message = "contractId 不能为空" });

        try
        {
            var count = await _generationService.GenerateAsync(
                request.ContractId,
                request.PeriodFrom,
                request.PeriodTo,
                ct);

            return Ok(new
            {
                message = $"应收已成功生成，共 {count} 条",
                contractId = request.ContractId,
                totalCreated = count
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class GenerateReceivablesRequest
{
    public Guid ContractId { get; set; }
    public string? PeriodFrom { get; set; }
    public string? PeriodTo { get; set; }
}
