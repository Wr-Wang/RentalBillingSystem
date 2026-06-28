using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/debit-notes")]
[Authorize]
public class DebitNotesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] Guid? contractId) => Ok(new List<object>());

    [HttpGet("{id}")]
    public IActionResult Get(Guid id) => Ok(new { });

    [HttpPost("generate")]
    public IActionResult Generate([FromBody] object dto) => Ok(new { message = "生成成功" });

    [HttpGet("{id}/pdf")]
    public IActionResult ExportPdf(Guid id) => File(Array.Empty<byte>(), "application/pdf", "bill.pdf");
}
