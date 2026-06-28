using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ApprovalsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var list = await _uow.ApprovalRequests.GetAllAsync(ct);
        return Ok(list.Where(a => a.Status == "Pending"));
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests(CancellationToken ct)
    {
        var list = await _uow.ApprovalRequests.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] RBS.Core.Entities.Approval.ApprovalRequest dto, CancellationToken ct)
    {
        await _uow.ApprovalRequests.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] Dictionary<string, string> body, CancellationToken ct)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.CompleteApproval("Approved");
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] Dictionary<string, string> body, CancellationToken ct)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        entity.CompleteApproval("Rejected");
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity.Records);
    }

    [HttpPost("{id}/retry-callback")]
    public IActionResult RetryCallback(Guid id) => Ok(new { });
}
