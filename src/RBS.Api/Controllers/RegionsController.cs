using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly StatsGovSyncProgress _statsGovProgress;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RegionsController> _logger;

    public RegionsController(
        IRegionService regionService,
        RegionApiSyncService syncService,
        StatsGovSyncProgress statsGovProgress,
        IServiceScopeFactory scopeFactory,
        ILogger<RegionsController> logger)
    {
        _regionService = regionService;
        _syncService = syncService;
        _statsGovProgress = statsGovProgress;
        _scopeFactory = scopeFactory;
        _logger = logger;
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

    /// <summary>
    /// 触发四级/五级数据同步（国家统计局 9+12 位编码，街道 4 万 + 社区 62 万条）。
    /// 任务在后台执行、接口立即返回，前端通过 <see cref="GetStatsGovProgress"/> 轮询实时进度。
    /// </summary>
    [HttpPost("syncStatsGov")]
    public IActionResult SyncStatsGov()
    {
        if (!_statsGovProgress.TryBegin())
            return Conflict(new { message = "已有一个四级/五级同步任务正在执行，请稍候" });

        _ = Task.Run(RunStatsGovImportAsync);
        return Accepted();
    }

    /// <summary>获取四级/五级数据同步的实时进度（前端轮询）</summary>
    [HttpGet("syncStatsGov/progress")]
    public IActionResult GetStatsGovProgress() => Ok(_statsGovProgress.GetSnapshot());

    private async Task RunStatsGovImportAsync()
    {
        try
        {
            // 用独立 scope 在请求结束后继续执行（服务为 Scoped，自行解析会随请求释放）
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<StatsGovRegionService>();
            var result = await service.SyncAllAsync();

            if (result.Done)
                _statsGovProgress.Succeed(result.Synced);
            else
                _statsGovProgress.Fail("同步数据下载失败或数据为空");
        }
        catch (Exception ex)
        {
            // 带上前进阶段信息，便于区分是下载超时还是入库阶段异常
            _logger.LogError(ex, "四级/五级同步失败");
            var stage = _statsGovProgress.GetSnapshot().Stage;
            _statsGovProgress.Fail(string.IsNullOrEmpty(stage) ? ex.Message : $"{stage}：{ex.Message}");
        }
    }

    /// <summary>从第三方 API 同步行政区划数据</summary>
    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromQuery] bool includeStreet = false)
    {
        var result = await _syncService.SyncAllAsync(includeStreet);
        return Ok(result);
    }
}
