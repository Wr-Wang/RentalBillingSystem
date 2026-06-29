using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ApiLogsController(AppDbContext db) => _db = db;

    private bool IsSuperAdmin => User.FindFirst("IsSuperAdmin")?.Value == "True";

    private IActionResult? RequireSuperAdmin()
    {
        if (!IsSuperAdmin) return Forbid();
        return null;
    }

    /// <summary>
    /// 分页查询 API 日志（仅超管）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? method = null,
        [FromQuery] string? path = null,
        [FromQuery] int? statusCode = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var query = _db.ApiLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(method))
            query = query.Where(l => l.HttpMethod == method);
        if (!string.IsNullOrEmpty(path))
            query = query.Where(l => l.Path.Contains(path));
        if (statusCode.HasValue)
            query = query.Where(l => l.StatusCode == statusCode.Value);
        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);
        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new
        {
            items = items.Select(l => new
            {
                l.Id, l.HttpMethod, l.Path, l.QueryString,
                l.StatusCode, l.DurationMs, l.IpAddress,
                l.UserId, l.UserDisplayName, l.CreatedAt
            }),
            total,
            page,
            pageSize
        });
    }

    /// <summary>
    /// 查看单条日志详情（含请求/响应体，仅超管）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var log = await _db.ApiLogs.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);
        if (log == null) return NotFound();
        return Ok(log);
    }

    /// <summary>
    /// 删除单条日志（仅超管）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        var log = await _db.ApiLogs.FindAsync(new object[] { id }, ct);
        if (log == null) return NotFound();
        _db.ApiLogs.Remove(log);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// 按日期范围删除 / 清空全部（仅超管）
    /// GET /api/apilogs                         ← 清空全部
    /// GET /api/apilogs?startDate=2026-06-01T00:00    ← 从指定时间开始删
    /// GET /api/apilogs?endDate=2026-06-28T23:59      ← 删到指定时间为止
    /// GET /api/apilogs?startDate=...&endDate=...     ← 按时间范围删除
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteByRange(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        IQueryable<ApiLog> query = _db.ApiLogs;

        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        _db.ApiLogs.RemoveRange(await query.ToListAsync(ct));
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
