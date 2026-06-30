using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalsController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    /// <summary>获取待审批列表</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var result = await _approvalService.GetPendingAsync(ct);
        return Ok(result);
    }

    /// <summary>获取我提交的请求</summary>
    [HttpGet("myrequests")]
    public async Task<IActionResult> GetMyRequests(CancellationToken ct)
    {
        var result = await _approvalService.GetMyRequestsAsync(ct);
        return Ok(result);
    }

    /// <summary>提交审批请求</summary>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitApprovalRequest request, CancellationToken ct)
    {
        var result = await _approvalService.SubmitAsync(request, ct);
        return Ok(result);
    }

    /// <summary>审批通过</summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequest body, CancellationToken ct)
    {
        var result = await _approvalService.ApproveAsync(id, body?.Comment, ct);
        return Ok(result);
    }

    /// <summary>审批驳回</summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest body, CancellationToken ct)
    {
        var result = await _approvalService.RejectAsync(id, body.Comment, ct);
        return Ok(result);
    }

    /// <summary>撤回审批（仅提交人可操作）</summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest? body, CancellationToken ct)
    {
        var result = await _approvalService.CancelAsync(id, body?.Reason, ct);
        return Ok(result);
    }

    /// <summary>获取审批历史（分页）</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] ApprovalHistoryQuery query, CancellationToken ct)
    {
        var result = await _approvalService.GetHistoryAsync(query, ct);
        return Ok(result);
    }

    /// <summary>获取审批详情</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _approvalService.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>获取审批历史记录</summary>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
    {
        var result = await _approvalService.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result.Records);
    }

    /// <summary>重试审批完成回调（预留）</summary>
    [HttpPost("{id}/retrycallback")]
    public IActionResult RetryCallback(Guid id) => Ok(new { });
}
