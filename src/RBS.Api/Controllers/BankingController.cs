using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Banking;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/banking")]
[Authorize]
public class BankingController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IBankingService _bankingService;

    public BankingController(IUnitOfWork uow, IBankingService bankingService)
    {
        _uow = uow;
        _bankingService = bankingService;
    }

    // ===== 银行流水 =====
    [HttpGet("statements")]
    public async Task<IActionResult> GetStatements([FromQuery] Guid? companyId, [FromQuery] string? status, CancellationToken ct)
    {
        var all = await _uow.BankStatements.GetAllAsync(ct);
        if (companyId.HasValue) all = all.Where(s => s.CompanyId == companyId.Value).ToList();
        if (!string.IsNullOrEmpty(status)) all = all.Where(s => s.Status == status).ToList();
        return Ok(all.OrderByDescending(s => s.TransactionDate));
    }

    [HttpPost("statements/import")]
    public async Task<IActionResult> ImportStatements([FromBody] ImportStatementsRequest request, CancellationToken ct)
    {
        if (request.CompanyId == Guid.Empty || request.Statements.Count == 0)
            return BadRequest(new { message = "参数错误" });

        var count = await _bankingService.ImportStatementsAsync(request.CompanyId, request.Statements, ct);
        return Ok(new { imported = count });
    }

    // ===== 对账会话 =====
    [HttpGet("reconciliations")]
    public async Task<IActionResult> GetReconciliations([FromQuery] Guid? companyId, CancellationToken ct)
    {
        var all = await _uow.BankReconciliations.GetAllAsync(ct);
        if (companyId.HasValue) all = all.Where(r => r.CompanyId == companyId.Value).ToList();
        return Ok(all.OrderByDescending(r => r.EndDate));
    }

    [HttpPost("reconciliations")]
    public async Task<IActionResult> CreateReconciliation([FromBody] BankReconciliation dto, CancellationToken ct)
    {
        await _uow.BankReconciliations.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPost("reconciliations/{id}/auto-match")]
    public async Task<IActionResult> AutoMatch(Guid id, CancellationToken ct)
    {
        try
        {
            var matches = await _bankingService.AutoMatchAsync(id, ct);
            return Ok(new { matched = matches.Count, matches });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reconciliations/{id}/complete")]
    public async Task<IActionResult> CompleteReconciliation(Guid id, CancellationToken ct)
    {
        try
        {
            await _bankingService.CompleteReconciliationAsync(id, ct);
            return Ok(new { message = "对账完成" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ===== 手动匹配 =====
    [HttpPost("matches")]
    public async Task<IActionResult> ManualMatch([FromBody] ManualMatchRequest request, CancellationToken ct)
    {
        if (request.StatementId == Guid.Empty || request.ReceiptId == Guid.Empty)
            return BadRequest(new { message = "参数错误" });

        try
        {
            var match = await _bankingService.ManualMatchAsync(request.StatementId, request.ReceiptId, request.Amount, ct);
            return Ok(match);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("matches")]
    public async Task<IActionResult> GetMatches([FromQuery] Guid? statementId, CancellationToken ct)
    {
        var all = await _uow.BankMatches.GetAllAsync(ct);
        if (statementId.HasValue) all = all.Where(m => m.BankStatementId == statementId.Value).ToList();
        return Ok(all);
    }
}

public class ImportStatementsRequest
{
    public Guid CompanyId { get; set; }
    public List<BankStatement> Statements { get; set; } = new();
}

public class ManualMatchRequest
{
    public Guid StatementId { get; set; }
    public Guid ReceiptId { get; set; }
    public decimal Amount { get; set; }
}
