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
        CancellationToken ct)
    {
        // 按公司查询（主列表）
        if (companyId != null)
        {
            var notes = await _debitNoteService.GetByCompanyAsync(companyId.Value, period, ct);
            // 按状态过滤（前端）
            if (!string.IsNullOrEmpty(status) && status != "All")
                notes = notes.Where(n => n.Status == status).ToList();
            return Ok(new { items = notes, total = notes.Count });
        }

        // 按合同查询（旧接口，兼容）
        if (contractId != null)
        {
            var notes = await _debitNoteService.GetByContractAsync(contractId.Value, ct);
            return Ok(notes);
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
}

public class GenerateDebitNoteRequest
{
    public Guid ContractId { get; set; }
    public string? Period { get; set; }
}
