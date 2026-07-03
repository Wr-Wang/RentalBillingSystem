using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IDbConnectionFactory _db;

    public AuditController(IAuditService auditService, IDbConnectionFactory db)
    {
        _auditService = auditService;
        _db = db;
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

        var auditTable = $"{tableName}_Audit";

        using var conn = _db.CreateConnection(); conn.Open();

        // 1. 检查审计表是否存在
        var tableExists = await conn.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM sys.tables WHERE name = @Name", new { Name = auditTable });
        if (tableExists == 0)
            return BadRequest(new { message = $"审计表 {auditTable} 不存在" });

        // 2. 读取指定版本的审计记录
        var auditRow = await conn.QuerySingleOrDefaultAsync<dynamic>(
            $"SELECT * FROM [{auditTable}] WHERE Id=@Id AND AuditVersionNo=@Ver",
            new { Id = recordId.Value.ToString(), Ver = versionNo.Value });

        if (auditRow == null)
            return NotFound(new { message = $"未找到版本 {versionNo} 的审计记录" });

        // 3. 获取最新版本（对比差异）
        var latestVersion = await conn.QuerySingleOrDefaultAsync<dynamic>(
            $"SELECT * FROM [{auditTable}] WHERE Id=@Id ORDER BY AuditVersionNo DESC",
            new { Id = recordId.Value.ToString() });

        var sourceRow = latestVersion ?? auditRow;

        // 4. 构建 UPDATE 语句恢复主表
        var updateFields = new List<string>();
        var updateParms = new Dictionary<string, object?>();
        var rowDict = ((IDictionary<string, object?>)sourceRow);

        foreach (var kv in rowDict)
        {
            var key = kv.Key;
            if (key is "AuditId" or "AuditAction" or "AuditVersionNo" or "AuditChangedAt" or "AuditChangedBy" or "RowVersion" or "Id")
                continue;
            updateFields.Add($"[{key}]=@{key}");
            updateParms[key] = kv.Value;
        }

        updateParms["Id"] = recordId.Value;
        var sql = $"UPDATE [{tableName}] SET {string.Join(", ", updateFields)} WHERE Id=@Id";
        await conn.ExecuteAsync(sql, updateParms);

        return Ok(new { message = $"已回滚到版本 {versionNo}", table = tableName, recordId = recordId.Value });
    }

    /// <summary>审计统计</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var result = await _auditService.GetStatsAsync(ct);
        return Ok(result);
    }
}
