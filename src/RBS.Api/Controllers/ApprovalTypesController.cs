using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/approval-types")]
[Authorize]
public class ApprovalTypesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ApprovalTypesController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _uow.ApprovalRequests.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object dto, CancellationToken ct) => Ok(new { });

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromBody] object dto) => NoContent();

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id) => NoContent();
}
