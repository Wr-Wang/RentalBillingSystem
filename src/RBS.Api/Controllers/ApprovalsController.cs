using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.Common;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;
    private readonly IUnitOfWork _uow;
    private readonly IServiceProvider _serviceProvider;

    public ApprovalsController(IApprovalService approvalService, IUnitOfWork uow, IServiceProvider serviceProvider)
    {
        _approvalService = approvalService;
        _uow = uow;
        _serviceProvider = serviceProvider;
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

    /// <summary>重试审批完成回调 — 重新触发审批后的业务回调逻辑</summary>
    [HttpPost("{id}/retrycallback")]
    public async Task<IActionResult> RetryCallback(Guid id, CancellationToken ct)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct);
        if (entity == null)
            return NotFound(new { message = "审批请求不存在" });

        if (entity.Status != "Approved" && entity.Status != "Rejected")
            return BadRequest(new { message = "仅已完成的审批可以重试回调" });

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<IEventHandler<ApprovalCompletedEvent>>();
        await handler.HandleAsync(
            new ApprovalCompletedEvent(id, entity.TargetEntityId, entity.TargetEntityType,
                entity.Status == "Approved" ? "Approved" : "Rejected"),
            ct);

        return Ok(new { message = "回调已重新触发" });
    }

    /// <summary>获取最近一次被驳回的审批数据（用于重新提交预填）</summary>
    [HttpGet("lastrejected")]
    public async Task<IActionResult> GetLastRejected([FromQuery] Guid targetEntityId, [FromQuery] string targetEntityType, CancellationToken ct)
    {
        var result = await _approvalService.GetLastRejectedAsync(targetEntityId, targetEntityType, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
