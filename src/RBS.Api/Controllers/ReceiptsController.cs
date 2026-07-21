using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IReceiptService _receiptService;

    public ReceiptsController(
        IUnitOfWork uow,
        IReceiptService receiptService)
    {
        _uow = uow;
        _receiptService = receiptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, [FromQuery] string? status,
        [FromQuery] Guid? contractId, CancellationToken ct)
    {
        var result = await _receiptService.GetAllAsync(companyId, status, contractId, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReceiptRequest request, CancellationToken ct)
    {
        var entity = string.IsNullOrWhiteSpace(request.ReceiptNo)
            ? Receipt.CreateNew(request.Amount, request.ReceivedDate, request.CompanyId, request.PaymentChannelId)
            : new Receipt(request.ReceiptNo, request.Amount, request.ReceivedDate, request.CompanyId);
        if (request.ContractId.HasValue) entity.LinkToContract(request.ContractId.Value);
        if (request.PaymentChannelId.HasValue && !string.IsNullOrWhiteSpace(request.ReceiptNo))
            entity.SetPaymentChannel(request.PaymentChannelId.Value);
        await _uow.Receipts.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _receiptService.ConfirmReceiptAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _receiptService.ReverseAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReceiptRejectRequest body, CancellationToken ct)
    {
        try
        {
            var entity = await _uow.Receipts.GetByIdAsync(id, ct);
            if (entity == null) return NotFound(new { message = "收款单不存在" });
            entity.Reject(body.Reason);
            await _uow.CommitAsync(ct);
            return Ok(entity);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("batchconfirm")]
    public async Task<IActionResult> BatchConfirm([FromBody] List<Guid> ids, CancellationToken ct)
    {
        var result = await _receiptService.BatchConfirmAsync(ids, ct);
        return Ok(result);
    }
}

public class ReceiptRejectRequest
{
    public string Reason { get; set; } = string.Empty;
}
