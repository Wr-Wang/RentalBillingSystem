using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
        => _notificationService = notificationService;

    /// <summary>分页查询通知列表</summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] NotificationQueryDto query,
        CancellationToken ct = default)
    {
        var result = await _notificationService.GetNotificationsAsync(query, ct);
        return Ok(result);
    }

    /// <summary>各分类未读计数</summary>
    [HttpGet("unreadcounts")]
    public async Task<IActionResult> GetUnreadCounts(CancellationToken ct = default)
    {
        var result = await _notificationService.GetUnreadCountsAsync(ct);
        return Ok(result);
    }

    /// <summary>单条标记已读</summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return BadRequest(new { message = "通知 ID 不能为空" });

        await _notificationService.MarkReadAsync(id, ct);
        return NoContent();
    }

    /// <summary>全部标记已读</summary>
    [HttpPut("readall")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
    {
        await _notificationService.MarkAllReadAsync(ct);
        return NoContent();
    }
}
