using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/autorenewconfig")]
[Authorize]
public class AutoRenewConfigController : ControllerBase
{
    private readonly IAutoRenewConfigService _service;

    public AutoRenewConfigController(IAutoRenewConfigService service) => _service = service;

    /// <summary>获取公司的自动续签配置</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid companyId, CancellationToken ct)
    {
        if (companyId == Guid.Empty)
            return BadRequest(new { message = "参数错误" });
        var config = await _service.GetByCompanyAsync(companyId, ct);
        return Ok(config);
    }

    /// <summary>保存自动续签配置（无则新增，有则覆盖）</summary>
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] AutoRenewConfig config, CancellationToken ct)
    {
        if (config.CompanyId == Guid.Empty)
            return BadRequest(new { message = "参数错误" });
        var result = await _service.SaveAsync(config, ct);
        return Ok(result);
    }
}
