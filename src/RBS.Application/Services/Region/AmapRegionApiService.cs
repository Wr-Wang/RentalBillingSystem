using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Region;

namespace RBS.Application.Services.Region;

/// <summary>
/// 高德地图 API 实现 — 行政区域查询
/// 文档：https://lbs.amap.com/api/webservice/guide/api/district
///
/// 使用前需在 appsettings.json 中配置：
/// {
///   "Amap": { "ApiKey": "你的高德 WebService Key" }
/// }
/// </summary>
public class AmapRegionApiService : IRegionApiService
{
    private readonly string _apiKey;
    private readonly ILogger<AmapRegionApiService> _logger;
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public AmapRegionApiService(string apiKey, ILogger<AmapRegionApiService> logger)
    {
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task<List<RegionApiDto>> GetChildrenAsync(string? parentCode)
    {
        try
        {
            // 顶层（省）：用 100000（中国）作为关键词获取所有省份
            // 子级：用父级 adcode 作为关键词获取其下一级
            var keywords = parentCode ?? "100000";
            var url = $"https://restapi.amap.com/v3/config/district"
                + $"?key={_apiKey}"
                + $"&keywords={keywords}"
                + $"&subdistrict=1"
                + $"&extensions=base";

            var response = await _http.GetFromJsonAsync<AmapDistrictResponse>(url);

            if (response?.Status != "1" || response.Districts == null || response.Districts.Count == 0)
            {
                _logger.LogWarning("高德 API 返回异常: {Info}", response?.Info ?? "无数据");
                return new List<RegionApiDto>();
            }

            var parent = response.Districts[0];
            var children = parent.Districts;

            if (children == null || children.Count == 0)
                return new List<RegionApiDto>();

            int parentLevel = LevelToInt(parent.Level);
            bool isCountryParent = parentLevel == 0; // 顶层查省时，父级是"中华人民共和国"

            return children.Select((c, i) => new RegionApiDto
            {
                Code = c.AdCode,
                Name = c.Name,
                ParentCode = parentCode,
                Level = parentLevel + 1,
                FullPath = isCountryParent ? c.Name : $"{parent.Name}/{c.Name}",
                SortOrder = i
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "高德 API 调用失败");
            return new List<RegionApiDto>();
        }
    }

    /// <summary>
    /// 获取省及下属市/区/街道全量数据（一次调用，subdistrict=3）
    /// 仅用于首次同步，后续增量用 GetChildrenAsync
    /// </summary>
    public async Task<List<RegionApiDto>> GetProvinceFullAsync(string provinceCode)
    {
        try
        {
            var url = $"https://restapi.amap.com/v3/config/district"
                + $"?key={_apiKey}"
                + $"&keywords={provinceCode}"
                + $"&subdistrict=3"
                + $"&extensions=base";

            var response = await _http.GetFromJsonAsync<AmapDistrictResponse>(url);
            if (response?.Status != "1" || response.Districts == null || response.Districts.Count == 0)
                return new List<RegionApiDto>();

            var result = new List<RegionApiDto>();
            var province = response.Districts[0];
            int level = LevelToInt(province.Level);
            var cities = province.Districts ?? new();

            foreach (var city in cities)
            {
                result.Add(new RegionApiDto
                {
                    Code = city.AdCode, Name = city.Name,
                    ParentCode = provinceCode, Level = level + 1,
                    FullPath = $"{province.Name}/{city.Name}", SortOrder = 0
                });

                var districts = city.Districts ?? new();
                foreach (var dist in districts)
                {
                    result.Add(new RegionApiDto
                    {
                        Code = dist.AdCode, Name = dist.Name,
                        ParentCode = city.AdCode, Level = level + 2,
                        FullPath = $"{province.Name}/{city.Name}/{dist.Name}", SortOrder = 0
                    });

                    // 高德 API 中街道的 adcode 与区级相同，须生成唯一编码
                    var streets = dist.Districts ?? new();
                    for (int si = 0; si < streets.Count; si++)
                    {
                        var street = streets[si];
                        // 合成唯一编码：区代码 + 3位序号（如 440105001）
                        var uniqueCode = dist.AdCode + (si + 1).ToString("D3");
                        result.Add(new RegionApiDto
                        {
                            Code = uniqueCode,
                            Name = street.Name,
                            ParentCode = dist.AdCode,
                            Level = level + 3,
                            FullPath = $"{province.Name}/{city.Name}/{dist.Name}/{street.Name}",
                            SortOrder = si
                        });
                    }
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取省全量数据失败");
            return new List<RegionApiDto>();
        }
    }

    public async Task<List<AddressSuggestionDto>> SearchAsync(string keyword, string? cityCode = null)
    {
        try
        {
            var url = $"https://restapi.amap.com/v3/assistant/inputtips"
                + $"?key={_apiKey}"
                + $"&keywords={Uri.EscapeDataString(keyword)}"
                + $"&type=120000"
                + (cityCode != null ? $"&city={cityCode}" : "");

            var response = await _http.GetFromJsonAsync<AmapInputTipsResponse>(url);

            if (response?.Status != "1" || response.Tips == null)
                return new List<AddressSuggestionDto>();

            return response.Tips
                .Where(t => !string.IsNullOrWhiteSpace(t.Address))
                .Select(t => new AddressSuggestionDto
                {
                    FormattedAddress = t.Address ?? "",
                    ProvinceCode = "",
                    CityCode = t.CityCode ?? "",
                    DistrictCode = t.AdCode ?? "",
                    Street = t.Address
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "高德搜索 API 调用失败");
            return new List<AddressSuggestionDto>();
        }
    }

    private static int LevelToInt(string? level) => level switch
    {
        "country" => 0,
        "province" => 1,
        "city" => 2,
        "district" => 3,
        "street" => 4,
        _ => 0
    };
}

// ===== 高德 API 响应模型 =====

public class AmapDistrictResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("info")]
    public string? Info { get; set; }

    [JsonPropertyName("districts")]
    public List<AmapDistrict>? Districts { get; set; }
}

public class AmapDistrict
{
    [JsonPropertyName("adcode")]
    public string AdCode { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("level")]
    public string? Level { get; set; }

    [JsonPropertyName("districts")]
    public List<AmapDistrict>? Districts { get; set; }
}

public class AmapInputTipsResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("info")]
    public string? Info { get; set; }

    [JsonPropertyName("tips")]
    public List<AmapTip>? Tips { get; set; }
}

public class AmapTip
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("citycode")]
    public string? CityCode { get; set; }

    [JsonPropertyName("adcode")]
    public string? AdCode { get; set; }
}
