using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/approval-levels")]
[Authorize]
public class ApprovalLevelsController : ControllerBase
{
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromBody] object dto) => NoContent();

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id) => NoContent();
}
