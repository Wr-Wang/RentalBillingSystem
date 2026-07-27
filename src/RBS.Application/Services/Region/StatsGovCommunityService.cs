using System.Data;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Region;
using RBS.Core.Common;

namespace RBS.Application.Services.Region;

/// <summary>
/// 国家统计局社区/村级数据同步服务
/// 数据来源：GitHub modood/Administrative-divisions-of-China（源于国家统计局统计用区划代码年报）
/// 12 位编码规则：省2 + 市2 + 区2 + 街道3 + 社区3
/// 使用 SqlBulkCopy 批量写入 62 万条数据
/// </summary>
public class StatsGovCommunityService
{
    private readonly IRegionService _regionService;
    private readonly IBulkInserter _bulkInserter;
    private readonly ILogger<StatsGovCommunityService> _logger;
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(120) };

    private const string VILLAGES_URL = "https://raw.githubusercontent.com/modood/Administrative-divisions-of-China/master/dist/villages.json";

    public StatsGovCommunityService(
        IRegionService regionService,
        IBulkInserter bulkInserter,
        ILogger<StatsGovCommunityService> logger)
    {
        _regionService = regionService;
        _bulkInserter = bulkInserter;
        _logger = logger;
    }

    public async Task<SyncResult> SyncCommunitiesAsync()
    {
        var result = new SyncResult();
        _logger.LogInformation("开始同步社区/村级数据");

        // 拉取社区/村数据
        _logger.LogInformation("下载社区数据（约 62 万条）...");
        var villages = await _http.GetFromJsonAsync<List<VillageItem>>(VILLAGES_URL);
        if (villages == null || villages.Count == 0)
        {
            _logger.LogWarning("社区数据获取失败");
            return result;
        }
        _logger.LogInformation("下载完成，共 {Count} 条", villages.Count);

        // 构建 DataTable 用于批量写入
        var dt = new DataTable("Regions");
        dt.Columns.Add("Id", typeof(Guid));
        dt.Columns.Add("Code", typeof(string));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("ParentCode", typeof(string));
        dt.Columns.Add("Level", typeof(int));
        dt.Columns.Add("FullPath", typeof(string));
        dt.Columns.Add("SortOrder", typeof(int));
        dt.Columns.Add("CreatedBy", typeof(Guid));
        dt.Columns.Add("CreatedAt", typeof(DateTime));

        var now = DateTime.UtcNow;
        var sysUserId = Guid.Empty;

        foreach (var v in villages)
        {
            var row = dt.NewRow();
            row["Id"] = Guid.NewGuid();
            row["Code"] = v.Code;
            row["Name"] = v.Name;
            row["ParentCode"] = v.StreetCode;
            row["Level"] = 5;
            row["FullPath"] = DBNull.Value;
            row["SortOrder"] = 0;
            row["CreatedBy"] = sysUserId;
            row["CreatedAt"] = DateTime.UtcNow;
            dt.Rows.Add(row);
        }

        _logger.LogInformation("开始批量写入 {Count} 条社区数据...", villages.Count);
        await _bulkInserter.BulkInsertAsync("Regions", dt);
        _logger.LogInformation("社区数据写入完成");

        result.Synced = villages.Count;
        result.Progress.Add($"✅ 社区/村级：{villages.Count} 条");
        result.Done = true;
        return result;
    }

    private class VillageItem
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string StreetCode { get; set; } = "";
    }
}
