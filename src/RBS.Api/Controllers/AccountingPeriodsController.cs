using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountingPeriodsController : ControllerBase
{
    private readonly IAccountingPeriodService _service;
    public AccountingPeriodsController(IAccountingPeriodService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Open([FromBody] OpenPeriodRequest request, CancellationToken ct)
    {
        var result = await _service.OpenPeriodAsync(request.Period, ct);
        return Ok(result);
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        await _service.ClosePeriodAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id}/reopen")]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken ct)
    {
        await _service.ReopenPeriodAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id}/lock")]
    public async Task<IActionResult> Lock(Guid id, CancellationToken ct)
    {
        await _service.LockPeriodAsync(id, ct);
        return NoContent();
    }
}

public class OpenPeriodRequest
{
    public string Period { get; set; } = string.Empty;
}
