using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/monitor")]
[Authorize]
public class TaskMonitorController : ControllerBase
{
    private readonly ITaskMonitorService _monitorService;

    public TaskMonitorController(ITaskMonitorService monitorService)
    {
        _monitorService = monitorService;
    }

    // ===== Dashboard 总览 =====

    /// <summary>获取执行总览统计</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken ct)
        => Ok(await _monitorService.GetDashboardStatsAsync(ct));

    /// <summary>获取成功率趋势</summary>
    [HttpGet("dashboard/trend")]
    public async Task<IActionResult> GetSuccessRateTrend(
        [FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await _monitorService.GetSuccessRateTrendAsync(days, ct));

    /// <summary>获取各任务平均耗时</summary>
    [HttpGet("dashboard/duration")]
    public async Task<IActionResult> GetTaskAvgDuration(
        [FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await _monitorService.GetTaskAvgDurationAsync(days, ct));

    /// <summary>获取失败原因聚合</summary>
    [HttpGet("dashboard/failures")]
    public async Task<IActionResult> GetFailureAggregation(
        [FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await _monitorService.GetFailureAggregationAsync(days, ct));

    // ===== 执行日志 =====

    /// <summary>分页查询执行日志</summary>
    [HttpGet("logs")]
    public async Task<IActionResult> QueryTaskLogs(
        [FromQuery] TaskLogQuery query, CancellationToken ct)
        => Ok(await _monitorService.QueryTaskLogsAsync(query, ct));

    /// <summary>获取单条日志详情（含步骤）</summary>
    [HttpGet("logs/{id}")]
    public async Task<IActionResult> GetTaskLogDetail(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _monitorService.GetTaskLogDetailAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "任务日志不存在" });
        }
    }

    /// <summary>反转预览</summary>
    [HttpPost("logs/{id}/previewreverse")]
    public async Task<IActionResult> PreviewReverse(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _monitorService.PreviewReverseAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "任务日志不存在" });
        }
    }
}
