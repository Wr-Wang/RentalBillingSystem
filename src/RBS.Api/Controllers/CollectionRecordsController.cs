using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/collection-records")]
[Authorize]
public class CollectionRecordsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] Guid? contractId) => Ok(new List<object>());

    [HttpPost("manual")]
    public IActionResult Manual([FromBody] object dto) => Ok(new { message = "催缴任务已创建" });
}
