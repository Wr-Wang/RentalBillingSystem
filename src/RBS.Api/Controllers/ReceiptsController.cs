using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAutoVoucherService _autoVoucher;
    public ReceiptsController(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql, IAutoVoucherService autoVoucher)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
        _autoVoucher = autoVoucher;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, CancellationToken ct)
    {
        if (companyId == null) return Ok(new List<object>());
        var list = await _uow.Receipts.GetPendingConfirmAsync(companyId.Value, ct);
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

        // 自动生成会计凭证
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
    {
        int count = 0;
        foreach (var id in ids)
        {
            try
            {
                var entity = await _uow.Receipts.GetByIdAsync(id, ct);
                if (entity != null && entity.Status == "Pending")
                {
                    entity.Confirm(Guid.Empty);
                    count++;
                }
            }
            catch { /* 单个失败不影响其余 */ }
        }
        await _uow.CommitAsync(ct);
        return Ok(new { confirmed = count });
    }

    /// <summary>冲销已确认的收款 — 先反转应收，再取消收款（在 Service 层编排）</summary>
    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] Dictionary<string, string>? body, CancellationToken ct)
    {
        var entity = await _uow.Receipts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound(new { message = "收款不存在" });

        using var conn = _db.CreateConnection(); conn.Open();
        var allocRows = await conn.QueryAsync(
            "SELECT ReceivablePlanId, Amount FROM ReceiptAllocations WHERE ReceiptId=@Id",
            new { Id = id });

        foreach (var row in allocRows)
        {
            var plan = await _uow.ReceivablePlans.GetByIdAsync((Guid)row.ReceivablePlanId, ct);
            plan?.ReversePayment((decimal)row.Amount);
        }

        await conn.ExecuteAsync(_sql.Get("Lease.Delete.ReceiptAllocation.ByReceiptId"), new { Id = id });
        entity.Cancel();
        await _uow.CommitAsync(ct);

        return Ok(new { message = "冲销成功", receiptId = id });
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
