using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalLevelsController : ControllerBase
{
    private readonly IApprovalTypeService _service;
    public ApprovalLevelsController(IApprovalTypeService service) => _service = service;

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApprovalLevelRequest request, CancellationToken ct)
    {
        await _service.UpdateLevelAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteLevelAsync(id, ct);
        return NoContent();
    }
}
