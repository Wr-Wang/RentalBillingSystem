using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Region;

namespace RBS.Application.Services.Region;

/// <summary>
/// 第三方 API 桩实现（未配置真实 API 时使用）
/// 返回空列表，避免启动时因 API 不可用而报错
/// 替换为真实实现（如 AmapRegionApiService）后即可生效
/// </summary>
public class RegionApiStubService : IRegionApiService
{
    public Task<List<RegionApiDto>> GetChildrenAsync(string? parentCode)
    {
        return Task.FromResult(new List<RegionApiDto>());
    }

    public Task<List<AddressSuggestionDto>> SearchAsync(string keyword, string? cityCode = null)
    {
        return Task.FromResult(new List<AddressSuggestionDto>());
    }

    public Task<List<RegionApiDto>> GetProvinceFullAsync(string provinceCode)
    {
        return Task.FromResult(new List<RegionApiDto>());
    }
}
