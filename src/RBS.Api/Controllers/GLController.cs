using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

/// <summary>
/// 总账 — 按期间查询科目级期初余额、本期发生额、累计发生额、期末余额
/// DDD 架构：Presentation Layer，仅处理 HTTP 请求/响应，委托给 Application Service
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GLController : ControllerBase
{
    private readonly IGLBalanceService _glService;
    private readonly ITenantService _tenant;
    private static readonly HashSet<string> ValidSourceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Receipt", "JournalPost", "BillJob", "Reverse", "SettleOffset"
    };

    public GLController(IGLBalanceService glService, ITenantService tenant)
    {
        _glService = glService;
        _tenant = tenant;
    }

    /// <summary>
    /// 总账余额表 — 按期间查询所有科目的期初/本期/累计/期末余额
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBalances(
        [FromQuery] string? period,
        [FromQuery] string? subjectCode,
        [FromQuery] int? subjectLevel,
        [FromQuery] string? contractNo,
        [FromQuery] string? sourceType,
        [FromQuery] bool hideZero = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(period))
            return BadRequest(new { message = "请提供会计期间(period)，格式 yyyy-MM" });

        var companyId = _tenant.EffectiveCompanyId;
        if (companyId == null)
            return Ok(new { period, items = Array.Empty<object>(), totals = new { } });

        if (!string.IsNullOrEmpty(sourceType) && !ValidSourceTypes.Contains(sourceType))
            return BadRequest(new { message = $"无效的来源类型: {sourceType}，有效值: {string.Join(", ", ValidSourceTypes)}" });

        var result = await _glService.GetBalancesAsync(
            companyId.Value, period, subjectCode, subjectLevel,
            contractNo, sourceType, hideZero, ct);

        return Ok(new { period, items = result.Items, totals = result.Totals });
    }

    /// <summary>
    /// 总账明细 — 按科目+期间查询明细分录，按合同号分组
    /// </summary>
    [HttpGet("detail")]
    public async Task<IActionResult> GetDetail(
        [FromQuery] string? period,
        [FromQuery] string? subjectCode,
        [FromQuery] string? contractNo,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(period))
            return BadRequest(new { message = "请提供会计期间(period)" });
        if (string.IsNullOrEmpty(subjectCode))
            return BadRequest(new { message = "请提供科目编码(subjectCode)" });

        var companyId = _tenant.EffectiveCompanyId;
        if (companyId == null)
            return Ok(new { subjectCode, period, entries = Array.Empty<object>(), groupedByContract = Array.Empty<object>() });

        var result = await _glService.GetDetailAsync(
            companyId.Value, period, subjectCode, contractNo, ct);

        return Ok(result);
    }
}
