using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Region;
using RBS.Application.Services.Region;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegionsController : ControllerBase
{
    private readonly IRegionService _regionService;
    private readonly RegionApiSyncService _syncService;
    private readonly StatsGovRegionService _statsGovService;

    public RegionsController(IRegionService regionService, RegionApiSyncService syncService, StatsGovRegionService statsGovService)
    {
        _regionService = regionService;
        _syncService = syncService;
        _statsGovService = statsGovService;
    }

    /// <summary>获取省份列表</summary>
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces()
    {
        var result = await _regionService.GetProvincesAsync();
        return Ok(result);
    }

    /// <summary>根据父代码获取子级列表（级联选择用）</summary>
    [HttpGet("children")]
    public async Task<IActionResult> GetChildren([FromQuery] string parentCode)
    {
        var result = await _regionService.GetChildrenAsync(parentCode);
        return Ok(result);
    }

    /// <summary>根据代码获取区域信息</summary>
    [HttpGet("byCode")]
    public async Task<IActionResult> GetByCode([FromQuery] string code)
    {
        var result = await _regionService.GetByCodeAsync(code);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>搜索区域（按名称/代码模糊查询）</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? keyword)
    {
        var result = await _regionService.SearchAsync(keyword);
        return Ok(result);
    }

    /// <summary>获取所有区域（管理后台用）</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _regionService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>新增或更新区域</summary>
    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] RegionDto dto)
    {
        await _regionService.UpsertAsync(dto);
        return Ok();
    }

    /// <summary>删除区域（含子级）</summary>
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        await _regionService.DeleteAsync(code);
        return NoContent();
    }

    /// <summary>同步四级/五级数据（国家统计局 9+12 位编码，街道 4 万 + 社区 62 万条）</summary>
    [HttpPost("syncStatsGov")]
    public async Task<IActionResult> SyncStatsGov()
    {
        var result = await _statsGovService.SyncAllAsync();
        return Ok(result);
    }

    /// <summary>从第三方 API 同步行政区划数据</summary>
    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromQuery] bool includeStreet = false)
    {
        var result = await _syncService.SyncAllAsync(includeStreet);
        return Ok(result);
    }
}
