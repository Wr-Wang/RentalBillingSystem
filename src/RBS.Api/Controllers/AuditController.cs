using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>获取所有已配置的审计表清单（供前端下拉列表动态加载）</summary>
    [HttpGet("tables")]
    public IActionResult GetAuditTables()
    {
        var tables = _auditService.GetAuditTables();
        return Ok(tables);
    }

    /// <summary>分页查询审计历史</summary>
    [HttpGet("{tableName}/history")]
    public async Task<IActionResult> GetHistory(
        string tableName,
        [FromQuery] string? recordId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = new AuditQuery
        {
            TableName = tableName,
            RecordId = recordId,
            Page = page,
            PageSize = pageSize,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await _auditService.GetHistoryAsync(query, ct);
        return Ok(result);
    }

    /// <summary>版本对比</summary>
    [HttpGet("{tableName}/compare")]
    public async Task<IActionResult> Compare(
        string tableName,
        [FromQuery] string? recordId,
        [FromQuery] int? v1,
        [FromQuery] int? v2,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(recordId) || !v1.HasValue || !v2.HasValue)
            return BadRequest(new { message = "参数不完整：需要 recordId、v1、v2" });

        var result = await _auditService.CompareAsync(tableName, recordId, v1.Value, v2.Value, ct);
        return Ok(result);
    }

    /// <summary>回滚到指定版本 — 从 _Audit 表读取版本数据，恢复主表</summary>
    [HttpPost("{tableName}/rollback")]
    public async Task<IActionResult> Rollback(string tableName, [FromQuery] Guid? recordId, [FromQuery] int? versionNo, CancellationToken ct)
    {
        if (recordId == null || !versionNo.HasValue)
            return BadRequest(new { message = "需要 recordId 和 versionNo" });

        var result = await _auditService.RollbackAsync(tableName, recordId.Value.ToString(), versionNo.Value, ct);

        if (result.ErrorCode == "TABLE_NOT_FOUND")
            return BadRequest(new { message = result.ErrorMessage });
        if (result.ErrorCode == "VERSION_NOT_FOUND")
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = result.ErrorMessage, table = result.Table, recordId = result.RecordId });
    }

    /// <summary>审计统计</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var result = await _auditService.GetStatsAsync(ct);
        return Ok(result);
    }
}
