using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Common;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiLogsController : ControllerBase
{
    private readonly IApiLogService _apiLogService;
    public ApiLogsController(IApiLogService apiLogService) { _apiLogService = apiLogService; }

    private bool IsSuperAdmin => User.FindFirst("IsSuperAdmin")?.Value == "True";
    private IActionResult? RequireSuperAdmin() => IsSuperAdmin ? null : Forbid();

    /// <summary>
    /// API 日志列表查询（默认近 7 天，排除 RequestBody/ResponseBody 大字段）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? method = null, [FromQuery] string? path = null,
        [FromQuery] int? statusCode = null, [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var (rows, total) = await _apiLogService.GetListAsync(
            page, pageSize, method, path, statusCode, userId, startDate, endDate, ct);

        return Ok(new
        {
            items = rows.Select(r => new
            {
                id = r.Id,
                httpMethod = r.HttpMethod,
                path = r.ApiPath,
                statusCode = r.StatusCode,
                durationMs = r.DurationMs,
                ipAddress = r.ClientIp,
                userId = r.UserId,
                userDisplayName = (string?)null,
                createdAt = r.RequestAt.AddHours(8)  // UTC 转东八区
            }),
            total, page, pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var log = await _apiLogService.GetDetailAsync(id, ct);
        if (log == null) return NotFound();

        return Ok(new
        {
            id = log.Id,
            userId = log.UserId,
            httpMethod = log.HttpMethod,
            path = log.ApiPath,
            queryString = log.QueryString,
            requestBody = log.RequestBody,
            statusCode = log.StatusCode,
            responseBody = log.ResponseBody,
            durationMs = log.DurationMs,
            ipAddress = log.ClientIp,
            userAgent = log.UserAgent,
            userDisplayName = log.UserDisplayName,
            createdAt = log.RequestAt.AddHours(8)  // UTC 转东八区
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        await _apiLogService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteByRange(
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        await _apiLogService.DeleteByRangeAsync(startDate, endDate, ct);
        return NoContent();
    }
}
