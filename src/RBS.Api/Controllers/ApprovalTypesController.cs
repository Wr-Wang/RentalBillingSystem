using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalTypesController : ControllerBase
{
    private readonly IApprovalTypeService _service;
    public ApprovalTypesController(IApprovalTypeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetListAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApprovalTypeRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApprovalTypeRequest request, CancellationToken ct)
    {
        await _service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{typeId}/levels")]
    public async Task<IActionResult> GetLevels(Guid typeId, CancellationToken ct)
    {
        var result = await _service.GetLevelsAsync(typeId, ct);
        return Ok(result);
    }

    [HttpPost("{typeId}/levels")]
    public async Task<IActionResult> CreateLevel(Guid typeId, [FromBody] CreateApprovalLevelRequest request, CancellationToken ct)
    {
        request.ApprovalTypeId = typeId;
        var result = await _service.CreateLevelAsync(request, ct);
        return Ok(result);
    }
}
