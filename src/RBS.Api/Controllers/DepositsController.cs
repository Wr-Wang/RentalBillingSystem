using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepositsController : ControllerBase
{
    private readonly IDepositService _depositService;

    public DepositsController(IDepositService depositService) => _depositService = depositService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        if (contractId == null)
            return Ok(new List<object>());

        var logs = await _depositService.GetByContractAsync(contractId.Value, ct);
        return Ok(logs);
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] DepositRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty || request.Amount <= 0)
            return BadRequest(new { message = "参数错误" });

        try
        {
            var log = await _depositService.ReturnAsync(request.ContractId, request.Amount, request.Remark, ct);
            return Ok(new { message = "退还成功", balance = log.Balance });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("deduct")]
    public async Task<IActionResult> Deduct([FromBody] DepositRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty || request.Amount <= 0)
            return BadRequest(new { message = "参数错误" });

        try
        {
            var log = await _depositService.DeductAsync(request.ContractId, request.Amount, request.Remark, ct);
            return Ok(new { message = "扣除成功", balance = log.Balance });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class DepositRequest
{
    public Guid ContractId { get; set; }
    public decimal Amount { get; set; }
    public string? Remark { get; set; }
}
