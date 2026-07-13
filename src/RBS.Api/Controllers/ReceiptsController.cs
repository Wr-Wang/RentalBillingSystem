using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
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
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ReceiptsController(
        IUnitOfWork uow, IAutoVoucherService autoVoucher,
        IReceiptService receiptService,
        IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _autoVoucher = autoVoucher;
        _receiptService = receiptService;
        _db = db;
        _sql = sql;
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
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            // 1. 乐观锁更新收款单状态（仅 Pending 可确认）
            var updated = await conn.ExecuteAsync(
                "UPDATE Receipts SET Status='Confirmed', UpdatedAt=GETUTCDATE() WHERE Id=@Id AND Status='Pending'",
                new { Id = id }, tx);

            if (updated == 0)
            {
                var receipt = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT Status FROM Receipts WHERE Id=@Id", new { Id = id }, tx);
                if (receipt == null) return NotFound();
                return BadRequest(new { message = $"收款单状态为「{(string)receipt.Status}」，仅待确认状态可确认" });
            }

            // 2. 同一事务内生成凭证（含 Voucher + JE + PrepaidBalance）
            await _autoVoucher.GenerateFromReceiptAsync(conn, tx, id, ct);

            tx.Commit();
            return Ok(new { message = "已确认" });
        }
        catch (InvalidOperationException ex)
        {
            tx.Rollback();
            return BadRequest(new { message = ex.Message });
        }
        catch
        {
            tx.Rollback();
            throw;
        }
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

    [HttpPost("batchconfirm")]
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
