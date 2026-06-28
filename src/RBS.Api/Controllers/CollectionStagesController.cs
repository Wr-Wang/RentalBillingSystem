using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/collection-stages")]
[Authorize]
public class CollectionStagesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(new List<object>());

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromBody] object dto) => NoContent();
}
