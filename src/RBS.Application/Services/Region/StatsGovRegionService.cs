using System.Data;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Region;
using RBS.Core.Common;

namespace RBS.Application.Services.Region;

/// <summary>
/// 国家统计局四级/五级数据同步服务
/// 数据来源：GitHub modood/Administrative-divisions-of-China（源于国家统计局年报）
///
/// 编码规则：
///   streets.json  — 9 位编码（省2+市2+区2+街道3）
///   villages.json — 12 位编码（省2+市2+区2+街道3+社区3）
/// </summary>
public class StatsGovRegionService
{
    private readonly IRegionService _regionService;
    private readonly IBulkInserter _bulkInserter;
    private readonly ILogger<StatsGovRegionService> _logger;
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(600) };

    private const string STREETS_URL = "https://raw.githubusercontent.com/modood/Administrative-divisions-of-China/master/dist/streets.json";
    private const string VILLAGES_URL = "https://raw.githubusercontent.com/modood/Administrative-divisions-of-China/master/dist/villages.json";

    public StatsGovRegionService(
        IRegionService regionService,
        IBulkInserter bulkInserter,
        ILogger<StatsGovRegionService> logger)
    {
        _regionService = regionService;
        _bulkInserter = bulkInserter;
        _logger = logger;
    }

    public async Task<SyncResult> SyncAllAsync()
    {
        var result = new SyncResult();
        var now = DateTime.UtcNow;
        var sysUserId = Guid.Empty;

        // 1. 下载街道数据（9 位编码）
        _logger.LogInformation("下载街道数据...");
        var streets = await _http.GetFromJsonAsync<List<StreetItem>>(STREETS_URL);
        if (streets == null) return result;
        _logger.LogInformation("街道数据 {Count} 条", streets.Count);

        // 2. 下载社区数据（12 位编码）
        _logger.LogInformation("下载社区数据...");
        var villages = await _http.GetFromJsonAsync<List<VillageItem>>(VILLAGES_URL);
        if (villages == null) return result;
        _logger.LogInformation("社区数据 {Count} 条", villages.Count);

        // 3. 加载已有省/市/区的全路径字典
        _logger.LogInformation("加载省/市/区全路径...");
        var pathCache = await BuildPathCacheAsync();

        // 4. 构建街道 DataTable（计算全路径）
        var dtStreets = BuildDataTable();
        foreach (var s in streets)
        {
            var parentPath = pathCache.TryGetValue(s.AreaCode, out var pp) ? pp : "";
            var row = dtStreets.NewRow();
            row["Id"] = Guid.NewGuid();
            row["Code"] = s.Code;
            row["Name"] = s.Name;
            row["ParentCode"] = s.AreaCode;
            row["Level"] = 4;
            row["FullPath"] = string.IsNullOrEmpty(parentPath) ? s.Name : $"{parentPath}/{s.Name}";
            row["SortOrder"] = 0;
            row["CreatedBy"] = sysUserId;
            row["CreatedAt"] = now;

            // 缓存街道全路径，供社区级使用
            pathCache[s.Code] = (string)row["FullPath"];
            dtStreets.Rows.Add(row);
        }

        // 5. 构建社区 DataTable（计算全路径）
        var dtVillages = BuildDataTable();
        foreach (var v in villages)
        {
            var parentPath = pathCache.TryGetValue(v.StreetCode, out var pp) ? pp : "";
            var row = dtVillages.NewRow();
            row["Id"] = Guid.NewGuid();
            row["Code"] = v.Code;
            row["Name"] = v.Name;
            row["ParentCode"] = v.StreetCode;
            row["Level"] = 5;
            row["FullPath"] = string.IsNullOrEmpty(parentPath) ? v.Name : $"{parentPath}/{v.Name}";
            row["SortOrder"] = 0;
            row["CreatedBy"] = sysUserId;
            row["CreatedAt"] = now;
            dtVillages.Rows.Add(row);
        }

        // 6. 先清空已有四级五级数据（避免编码冲突）
        _logger.LogInformation("清理旧的四级/五级数据...");
        await _regionService.DeleteByLevelAsync(4);
        await _regionService.DeleteByLevelAsync(5);

        // 7. 批量写入街道（4.1 万条）
        _logger.LogInformation("写入街道 {Count} 条...", streets.Count);
        await _bulkInserter.BulkInsertAsync("Regions", dtStreets);

        // 8. 批量写入社区（62 万条）
        _logger.LogInformation("写入社区 {Count} 条...", villages.Count);
        await _bulkInserter.BulkInsertAsync("Regions", dtVillages);

        _logger.LogInformation("四级/五级数据同步完成");
        result.Synced = streets.Count + villages.Count;
        result.Progress.Add($"✅ 街道级：{streets.Count} 条");
        result.Progress.Add($"✅ 社区/村级：{villages.Count} 条");
        result.Done = true;
        return result;
    }

    /// <summary>从数据库加载已有区域的全路径字典（Level 1-3）</summary>
    private async Task<Dictionary<string, string>> BuildPathCacheAsync()
    {
        var all = await _regionService.GetAllAsync();
        return all.Where(r => r.Level <= 3 && !string.IsNullOrEmpty(r.FullPath))
                  .DistinctBy(r => r.Code)
                  .ToDictionary(r => r.Code, r => r.FullPath!);
    }

    private static DataTable BuildDataTable()
    {
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
        return dt;
    }

    private class StreetItem
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string AreaCode { get; set; } = "";
    }

    private class VillageItem
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string StreetCode { get; set; } = "";
    }
}
