using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    [HttpGet("{tableName}/history")]
    public IActionResult GetHistory(string tableName, [FromQuery] Guid? recordId) => Ok(new List<object>());

    [HttpGet("{tableName}/compare")]
    public IActionResult Compare(string tableName, [FromQuery] Guid? recordId, [FromQuery] int? v1, [FromQuery] int? v2) => Ok(new { });

    [HttpPost("{tableName}/rollback")]
    public IActionResult Rollback(string tableName, [FromQuery] Guid? recordId, [FromQuery] int? versionNo) => Ok(new { message = "回滚成功" });

    [HttpGet("stats")]
    public IActionResult GetStats() => Ok(new { });
}
