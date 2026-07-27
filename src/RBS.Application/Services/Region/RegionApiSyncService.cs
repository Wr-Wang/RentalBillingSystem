using Microsoft.Extensions.Logging;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Region;

namespace RBS.Application.Services.Region;

/// <summary>
/// 第三方 API 同步服务
///
/// 策略：
/// 1. 省 → 市 → 区逐级同步，数据库已有的跳过
/// 2. 每同步一个省就记录一条进度，前端可分段展示
/// 3. 限流时自动重试（最多 3 次，间隔 3s）
/// 4. 多次同步可补全（已存在的跳过，只拉缺失数据）
/// </summary>
public class RegionApiSyncService
{
    private readonly IRegionApiService _api;
    private readonly IRegionService _regionService;
    private readonly ILogger<RegionApiSyncService> _logger;

    public RegionApiSyncService(
        IRegionApiService api,
        IRegionService regionService,
        ILogger<RegionApiSyncService> logger)
    {
        _api = api;
        _regionService = regionService;
        _logger = logger;
    }

    public async Task<SyncResult> SyncAllAsync(bool includeStreet = false)
    {
        var result = new SyncResult();
        _logger.LogInformation("开始同步行政区划数据");

        // 1. 同步省
        var provinces = await CallWithRetryAsync(() => _api.GetChildrenAsync(null));
        if (provinces == null) return result;

        result.ProvinceCount = provinces.Count;
        await _regionService.BatchUpsertAsync(
            provinces.Select(p => ToDto(p, null)).ToList());
        result.Progress.Add($"✅ 省份 {provinces.Count} 个");

        // 2. 逐省同步市/区/街道（每省一次 API 调用）
        foreach (var p in provinces)
        {
            var hasChildren = await HasChildrenAsync(p.Code);
            if (hasChildren)
            {
                result.SkippedProvince(p.Name);
                continue;
            }

            await Task.Delay(1000);
            var allData = await CallWithRetryAsync(() => _api.GetProvinceFullAsync(p.Code));
            if (allData == null || allData.Count == 0)
            {
                result.RateLimitedProvince(p.Name);
                continue;
            }

            var inserted = await _regionService.BatchUpsertAsync(
                allData.Select(d => new RegionDto
                {
                    Code = d.Code, Name = d.Name, ParentCode = d.ParentCode,
                    Level = d.Level, FullPath = d.FullPath, SortOrder = d.SortOrder
                }).ToList());

            var cities = allData.Where(d => d.Level == 2).Count();
            var districts = allData.Where(d => d.Level == 3).Count();
            var streets = allData.Where(d => d.Level == 4).Count();

            var detail = $"{p.Name}：{cities} 市、{districts} 区";
            if (streets > 0) detail += $"，{streets} 街道";
            result.Progress.Add($"✅ {detail}");
        }

        result.Done = true;
        return result;
    }

    /// <summary>检查数据库是否已有子级数据</summary>
    private async Task<bool> HasChildrenAsync(string parentCode)
    {
        try
        {
            var children = await _regionService.GetChildrenAsync(parentCode);
            return children.Count > 0;
        }
        catch { return false; }
    }

    private async Task<List<RegionApiDto>?> CallWithRetryAsync(Func<Task<List<RegionApiDto>>> call)
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                var result = await call();
                if (result.Count > 0) return result;
            }
            catch { }
            await Task.Delay(3000);
        }
        return null;
    }

    private static RegionDto ToDto(RegionApiDto apiDto, string? parentCode) => new()
    {
        Code = apiDto.Code, Name = apiDto.Name,
        ParentCode = parentCode ?? apiDto.ParentCode,
        Level = apiDto.Level, FullPath = apiDto.FullPath,
        SortOrder = apiDto.SortOrder
    };
}

public class SyncResult
{
    public bool Done { get; set; }
    public int Synced { get; set; }
    public int ProvinceCount { get; set; }
    public int TotalSynced => ProvinceCount + CityCount + DistrictCount;
    public int CityCount { get; set; }
    public int DistrictCount { get; set; }
    public int RateLimited { get; set; }
    public List<string> Progress { get; set; } = new();
    public List<string> Skipped { get; set; } = new();

    public void SkippedProvince(string name) => Skipped.Add(name);
    public void RateLimitedProvince(string name) => Progress.Add($"⏳ {name}（接口限流，可下次同步补充）");
}
