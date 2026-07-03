using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IAutoVoucherService _autoVoucher;
    private readonly IReceiptService _receiptService;

    public ReceiptsController(IUnitOfWork uow, IAutoVoucherService autoVoucher, IReceiptService receiptService)
    {
        _uow = uow;
        _autoVoucher = autoVoucher;
        _receiptService = receiptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, [FromQuery] string? status, CancellationToken ct)
    {
        if (companyId == null) return Ok(new List<object>());

        if (string.IsNullOrEmpty(status) || status == "Pending")
        {
            var pending = await _uow.Receipts.GetPendingConfirmAsync(companyId.Value, ct);
            return Ok(pending);
        }

        var list = await _uow.Receipts.GetAllByCompanyAsync(companyId.Value, ct);
        if (!string.IsNullOrEmpty(status) && status != "All")
            list = list.Where(r => r.Status == status).ToList();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReceiptRequest request, CancellationToken ct)
    {
        var entity = new Receipt(request.ReceiptNo, request.Amount, request.ReceivedDate, request.CompanyId);
        if (request.ContractId.HasValue) entity.LinkToContract(request.ContractId.Value);
        await _uow.Receipts.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Receipts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.Confirm(Guid.Empty);
        await _uow.CommitAsync(ct);
        await _autoVoucher.GenerateFromReceiptAsync(id, ct);
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
        => Ok(await _receiptService.BatchConfirmAsync(ids, ct));

    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, CancellationToken ct)
    {
        try { return Ok(await _receiptService.ReverseAsync(id, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}

public class CreateReceiptRequest
{
    public string ReceiptNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ContractId { get; set; }
}
