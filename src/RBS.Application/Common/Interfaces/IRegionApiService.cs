using RBS.Application.DTOs.Region;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 第三方地址 API 服务接口
/// 对接高德/百度地图等外部服务，提供行政区划查询和地址搜索能力
/// </summary>
public interface IRegionApiService
{
    /// <summary>获取指定区域的子级列表（用于同步入库）</summary>
    /// <param name="parentCode">父级代码，null 获取省一级</param>
    Task<List<RegionApiDto>> GetChildrenAsync(string? parentCode);

    /// <summary>获取省及下属市/区/街道全量（subdistrict=3，一次调用拿四级）</summary>
    Task<List<RegionApiDto>> GetProvinceFullAsync(string provinceCode);

    /// <summary>地址搜索（用于前端输入自动提示）</summary>
    /// <param name="keyword">关键词</param>
    /// <param name="cityCode">限定城市代码（可选）</param>
    Task<List<AddressSuggestionDto>> SearchAsync(string keyword, string? cityCode = null);
}

/// <summary>第三方 API 返回的区域原始数据</summary>
public class RegionApiDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public int Level { get; set; }
    public string? FullPath { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>地址搜索建议 DTO</summary>
public class AddressSuggestionDto
{
    public string FormattedAddress { get; set; } = string.Empty;
    public string ProvinceCode { get; set; } = string.Empty;
    public string CityCode { get; set; } = string.Empty;
    public string DistrictCode { get; set; } = string.Empty;
    public string? Street { get; set; }
}
