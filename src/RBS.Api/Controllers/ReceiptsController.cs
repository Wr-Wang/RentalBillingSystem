using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
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
    private readonly IReceiptService _receiptService;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ReceiptsController(
        IUnitOfWork uow,
        IReceiptService receiptService,
        IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
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
                _sql.Get("Collection.Update.Receipt.Confirm"),
                new { Id = id }, tx);

            if (updated == 0)
            {
                var receipt = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    _sql.Get("Collection.Select.Receipt.StatusById"), new { Id = id }, tx);
                if (receipt == null) return NotFound();
                return BadRequest(new { message = $"收款单状态为「{(string)receipt.Status}」，仅待确认状态可确认" });
            }

            // 2. 更新合同欠款/预存余额
            var receiptInfo = await conn.QuerySingleAsync<dynamic>(
                _sql.Get("Receipt.Select.Receipt.WithContractBalance"),
                new { Id = id }, tx);
            if (receiptInfo != null)
            {
                var rContractId = (Guid?)receiptInfo.ContractId;
                if (rContractId == null) { tx.Commit(); return Ok(new { id }); }
                var cId = rContractId.Value;
                var amt = (decimal)receiptInfo.Amount;
                var outstanding = (decimal?)receiptInfo.OutstandingBalance ?? 0m;
                var offset = Math.Min(amt, outstanding); // 先冲欠款
                var overflow = amt - offset;              // 超出进预存
                if (offset > 0)
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                        new { Id = cId, Amt = -offset }, tx);
                if (overflow > 0)
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.PrepaidBalanceIncrement"),
                        new { Id = cId, Amt = overflow }, tx);
            }

            tx.Commit();
            return Ok(new { id });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(new { error = ex.Message });
        }
    }
}
