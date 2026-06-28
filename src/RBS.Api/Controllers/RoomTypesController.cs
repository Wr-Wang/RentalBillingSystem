using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/room-types")]
[Authorize]
public class RoomTypesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(new List<object>());

    [HttpPost]
    public IActionResult Create([FromBody] object dto) => Ok(new { });

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromBody] object dto) => NoContent();

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id) => NoContent();
}
