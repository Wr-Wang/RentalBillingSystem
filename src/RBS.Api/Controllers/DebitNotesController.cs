using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DebitNotesController : ControllerBase
{
    private readonly IDebitNoteService _debitNoteService;

    public DebitNotesController(IDebitNoteService debitNoteService) => _debitNoteService = debitNoteService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? contractId,
        [FromQuery] string? period,
        [FromQuery] string? status,
        [FromQuery] string? keyword,
        CancellationToken ct)
    {
        // 按公司查询（主列表）
        if (companyId != null)
        {
            var filterStatus = !string.IsNullOrEmpty(status) && status != "All" ? status : null;
            var notes = await _debitNoteService.GetByCompanyAsync(companyId.Value, period, contractId, keyword, filterStatus, ct);
            return Ok(new { items = notes, total = notes.Count });
        }

        // 按合同查询
        if (contractId != null)
        {
            var notes = await _debitNoteService.GetByContractAsync(contractId.Value, ct);
            return Ok(new { items = notes, total = notes.Count });
        }

        return Ok(new { items = new List<object>(), total = 0 });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var note = await _debitNoteService.GetByIdAsync(id, ct);
        if (note == null) return NotFound();
        return Ok(note);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateDebitNoteRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty || string.IsNullOrEmpty(request.Period))
            return BadRequest(new { message = "参数错误" });

        try
        {
            var note = await _debitNoteService.GenerateAsync(request.ContractId, request.Period, ct);
            return Ok(new { message = "生成成功", id = note.Id, noteNo = note.NoteNo });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> ExportPdf(Guid id, CancellationToken ct)
    {
        try
        {
            var pdf = await _debitNoteService.ExportPdfAsync(id, ct);
            return File(pdf, "application/pdf", $"bill-{id:N}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>作废账单</summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelDebitNoteRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
        try
        {
            await _debitNoteService.CancelAsync(id, request.Reason ?? "", userId, ct);
            return Ok(new { message = "账单已作废" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "账单不存在" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>删除账单（硬删，用于重新生成）</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _debitNoteService.DeleteAsync(id, ct);
            return Ok(new { message = "账单已删除" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "账单不存在" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class GenerateDebitNoteRequest
{
    public Guid ContractId { get; set; }
    public string? Period { get; set; }
}

public class CancelDebitNoteRequest
{
    public string? Reason { get; set; }
}
