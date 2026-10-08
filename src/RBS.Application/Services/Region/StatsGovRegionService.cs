using System.Data;
using System.IO.Compression;
using System.Text.Json;
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
///
/// 下载策略（避免大文件固定超时被掐断）：
///   1) 多镜像源依次兜底（直连 GitHub raw 在国内常被重置）
///   2) 整体不设请求超时（Timeout=Infinite），改用“每 90s 收不到新字节即判死并换源”
///   3) 先读响应头拿 Content-Length 作为总字节，流式边读边上报进度（字节）
///   4) 下载内容落本地缓存（bin/cache/statsgov）：一个月内直接复用，超期删除重下
/// </summary>
public class StatsGovRegionService
{
    private readonly IRegionService _regionService;
    private readonly IBulkInserter _bulkInserter;
    private readonly ILogger<StatsGovRegionService> _logger;
    private readonly StatsGovSyncProgress _progress;

    // 大文件下载专用客户端：整体超时无限，配合逐块读超时做“空闲判死”
    private static readonly HttpClient _downloadHttp = new() { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
    private static readonly TimeSpan ReadIdleTimeout = TimeSpan.FromSeconds(90);   // 连续多久无新字节判死
    private static readonly TimeSpan HeadersTimeout = TimeSpan.FromSeconds(60);    // 响应头等待上限

    // 与旧 GetFromJsonAsync 一致（Web 默认：JSON 键大小写不敏感）。
    // 文件键为 camelCase（code/name），C# 属性为 PascalCase（Code/Name），
    // 若用大小写敏感默认配置解析，会把每行都解析成空 Code/Name 并导致唯一索引重复空键。
    private static readonly JsonSerializerOptions _webJson = new(JsonSerializerDefaults.Web);

    private const string STREETS_PATH = "dist/streets.json";
    private const string VILLAGES_PATH = "dist/villages.json";

    // 本地文件缓存：一个月内的缓存直接复用，超期删除后重新下载
    private static readonly TimeSpan CacheMaxAge = TimeSpan.FromDays(30);
    private static string CacheDir => Path.Combine(AppContext.BaseDirectory, "cache", "statsgov");

    // 镜像源兜底（{0} 为文件路径）。raw 直连在国内网络常被重置，故放在最后；
    // 代理源对 85MB 的 villages.json 同样有效（jsDelivr 系列有 20MB 上限会 403，故不采用）。
    private static readonly string[] MIRROR_TEMPLATES =
    {
        "https://ghproxy.net/https://raw.githubusercontent.com/modood/Administrative-divisions-of-China/master/{0}",
        "https://ghfast.top/https://raw.githubusercontent.com/modood/Administrative-divisions-of-China/master/{0}",
        "https://gh-proxy.com/https://raw.githubusercontent.com/modood/Administrative-divisions-of-China/master/{0}",
        "https://raw.githubusercontent.com/modood/Administrative-divisions-of-China/master/{0}",
    };

    public StatsGovRegionService(
        IRegionService regionService,
        IBulkInserter bulkInserter,
        ILogger<StatsGovRegionService> logger,
        StatsGovSyncProgress progress)
    {
        _regionService = regionService;
        _bulkInserter = bulkInserter;
        _logger = logger;
        _progress = progress;
    }

    public async Task<SyncResult> SyncAllAsync()
    {
        var result = new SyncResult();
        var now = RBS.Core.Common.ChinaTime.Now;
        var sysUserId = SystemUsers.DataSync;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        long downloadMs = 0, buildMs = 0, deleteMs = 0, writeMs = 0;

        // 1. 街道数据（9 位编码）——优先本地缓存，否则流式下载（进度=已收字节/Content-Length）
        _logger.LogInformation("加载街道数据...");
        var streets = await LoadJsonListAsync<StreetItem>(STREETS_PATH, "正在获取街道数据（9 位编码）");
        if (streets == null) return result;
        _logger.LogInformation("街道数据 {Count} 条", streets.Count);
        _progress.AddLine($"✅ 街道数据下载完成：{streets.Count:N0} 条");

        // 2. 社区数据（12 位编码）——同上
        _logger.LogInformation("加载社区数据...");
        var villages = await LoadJsonListAsync<VillageItem>(VILLAGES_PATH, "正在获取社区/村级数据（12 位编码）");
        if (villages == null) return result;
        _logger.LogInformation("社区数据 {Count} 条", villages.Count);
        _progress.AddLine($"✅ 社区/村级数据下载完成：{villages.Count:N0} 条");
        downloadMs = sw.ElapsedMilliseconds; // 两个 JSON 文件下载完毕，进入本地转换阶段

        // 3. 加载已有省/市/区的全路径字典
        _progress.SetStage("正在加载省/市/区全路径…");
        _logger.LogInformation("加载省/市/区全路径...");
        var pathCache = await BuildPathCacheAsync();

        // 4. 构建街道 DataTable（计算全路径）
        _progress.SetStage($"正在生成街道级数据表（{streets.Count:N0} 条）…", streets.Count);
        var dtStreets = BuildDataTable();
        long streetIdx = 0;
        foreach (var s in streets)
        {
            if (string.IsNullOrWhiteSpace(s.Code)) { _logger.LogWarning("街道数据存在空 Code 行，已跳过：{Name}", s.Name); continue; }
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

            if (++streetIdx % 5000 == 0) _progress.Report(streetIdx, streets.Count);
        }
        _progress.Report(streets.Count, streets.Count);
        _progress.AddLine($"✅ 街道级数据生成完成：{streets.Count:N0} 条");

        // 5. 构建社区 DataTable（计算全路径）
        _progress.SetStage($"正在生成社区/村级数据表（{villages.Count:N0} 条）…", villages.Count);
        var dtVillages = BuildDataTable();
        long villageIdx = 0;
        foreach (var v in villages)
        {
            if (string.IsNullOrWhiteSpace(v.Code)) { _logger.LogWarning("社区数据存在空 Code 行，已跳过：{Name}", v.Name); continue; }
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

            if (++villageIdx % 20000 == 0) _progress.Report(villageIdx, villages.Count);
        }
        _progress.Report(villages.Count, villages.Count);
        _progress.AddLine($"✅ 社区/村级数据生成完成：{villages.Count:N0} 条");
        buildMs = sw.ElapsedMilliseconds - downloadMs; // 内存构建 DataTable 结束

        // 6. 先清空已有四级五级数据（避免编码冲突）
        _progress.SetStage("正在删除旧的街道/社区数据…");
        _logger.LogInformation("清理旧的四级/五级数据...");
        await _regionService.DeleteByLevelAsync(4);
        await _regionService.DeleteByLevelAsync(5);
        _progress.AddLine("✅ 旧的四级/五级数据已清理");
        deleteMs = sw.ElapsedMilliseconds - downloadMs - buildMs; // 删除旧数据结束

        // 7. 批量写入街道（4.1 万条）
        _progress.SetStage($"正在批量写入街道级数据（{streets.Count:N0} 条）…", streets.Count);
        _logger.LogInformation("写入街道 {Count} 条...", streets.Count);
        await _bulkInserter.BulkInsertAsync("Regions", dtStreets,
            onRowsCopied: rows => _progress.Report(rows, streets.Count), bulkCopyTimeoutSeconds: 1800);
        _progress.Report(streets.Count, streets.Count);
        _progress.AddLine($"✅ 街道级写入完成：{streets.Count:N0} 条");

        // 8. 批量写入社区（62 万条）
        _progress.SetStage($"正在批量写入社区/村级数据（{villages.Count:N0} 条）…", villages.Count);
        _logger.LogInformation("写入社区 {Count} 条...", villages.Count);
        await _bulkInserter.BulkInsertAsync("Regions", dtVillages,
            onRowsCopied: rows => _progress.Report(rows, villages.Count), bulkCopyTimeoutSeconds: 1800);
        _progress.Report(villages.Count, villages.Count);
        _progress.AddLine($"✅ 社区/村级写入完成：{villages.Count:N0} 条");
        writeMs = sw.ElapsedMilliseconds - downloadMs - buildMs - deleteMs; // 批量写入结束

        _logger.LogInformation("四级/五级数据同步完成：总耗时 {Total}ms（下载 {Download}ms / 生成 {Build}ms / 删除 {Delete}ms / 写入 {Write}ms）",
            sw.ElapsedMilliseconds, downloadMs, buildMs, deleteMs, writeMs);
        result.Synced = streets.Count + villages.Count;
        result.Progress.Add($"✅ 街道级：{streets.Count:N0} 条");
        result.Progress.Add($"✅ 社区/村级：{villages.Count:N0} 条");
        result.Progress.Add($"⏱ 总耗时 {FormatDuration(sw.ElapsedMilliseconds)}：下载 {FormatDuration(downloadMs)}、生成数据 {FormatDuration(buildMs)}、删除旧数据 {FormatDuration(deleteMs)}、批量写入 {FormatDuration(writeMs)}");
        result.Done = true;
        return result;
    }

    /// <summary>
    /// 加载一个 JSON 列表：优先复用本地缓存（一个月内），过期则删除并重新下载。
    /// </summary>
    private async Task<List<TItem>?> LoadJsonListAsync<TItem>(string path, string stageText) where TItem : class
    {
        var fileName = Path.GetFileName(path);
        var cacheFile = Path.Combine(CacheDir, fileName);

        if (File.Exists(cacheFile))
        {
            var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFile);
            if (age <= CacheMaxAge)
            {
                // 缓存新鲜：直接使用，跳过下载
                try
                {
                    var sizeMb = new FileInfo(cacheFile).Length / 1024.0 / 1024.0;
                    _progress.SetStage($"{stageText} — 使用本地缓存（{File.GetLastWriteTime(cacheFile):yyyy-MM-dd}，{sizeMb:0.0} MB）…");
                    _logger.LogInformation("使用本地缓存 {File}（缓存于 {Time}）", cacheFile, File.GetLastWriteTime(cacheFile));
                    var cached = await ParseFileAsync<TItem>(cacheFile);
                    if (cached == null) throw new IOException("缓存内容解析为空。");
                    _progress.AddLine($"📦 使用本地缓存：{fileName}（{File.GetLastWriteTime(cacheFile):yyyy-MM-dd}）");
                    return cached;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "本地缓存 {File} 不可用，删除后重新下载", cacheFile);
                    _progress.AddLine($"⚠️ 本地缓存 {fileName} 损坏，已删除并重新下载");
                    TryDelete(cacheFile);
                }
            }
            else
            {
                // 超过一个月：删除后重新下载
                _logger.LogInformation("本地缓存 {File} 已超过 {Days} 天，删除后重新下载", cacheFile, CacheMaxAge.TotalDays);
                _progress.AddLine($"🗑 本地缓存 {fileName} 已超过 {CacheMaxAge.TotalDays:0} 天，删除并重新下载");
                TryDelete(cacheFile);
            }
        }

        return await DownloadToCacheAsync<TItem>(path, stageText, cacheFile);
    }

    /// <summary>
    /// 依次尝试各镜像源，流式下载并落盘缓存，再解析为 JSON 列表。
    /// 先读响应头拿 Content-Length 作为总字节数，边读边把“已收字节/总字节”上报给进度器；
    /// 整体不设固定超时（大文件慢速下载也能跑完），仅在连续 ReadIdleTimeout 收不到新字节时判死并切换下一个源。
    /// </summary>
    private async Task<List<TItem>?> DownloadToCacheAsync<TItem>(string path, string stageText, string cacheFile) where TItem : class
    {
        Directory.CreateDirectory(CacheDir);
        var tempFile = cacheFile + ".tmp";
        Exception? last = null;

        for (var i = 0; i < MIRROR_TEMPLATES.Length; i++)
        {
            var url = string.Format(MIRROR_TEMPLATES[i], path);
            var host = new Uri(url).Host;
            // 发请求前先置阶段：连响应头都没拿到就失败时，也能看出卡在哪个源
            var label = $"{stageText} — 源 {i + 1}/{MIRROR_TEMPLATES.Length}：{host}";
            _progress.SetStage(label, 0, "bytes");
            try
            {
                using var headersCts = new CancellationTokenSource(HeadersTimeout);
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _downloadHttp.SendAsync(
                    request, HttpCompletionOption.ResponseHeadersRead, headersCts.Token);
                response.EnsureSuccessStatusCode();

                var total = response.Content.Headers.ContentLength ?? 0;
                var isGzip = response.Content.Headers.ContentEncoding.Any(
                    e => e.Contains("gzip", StringComparison.OrdinalIgnoreCase));
                _progress.SetStage(label, total, "bytes"); // 下载进度按字节展示
                _logger.LogInformation("开始下载 {Url}，大小 {Size} 字节", url, total);

                await using var netStream = await response.Content.ReadAsStreamAsync();
                // 计数+空闲判死包装：读取过程中持续上报字节进度
                using var counting = new ProgressReadStream(netStream, read => _progress.Report(read, total));

                // 先写临时文件（gzip 则解压后落盘，缓存里始终是明文 JSON）
                await using (var file = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
                {
                    if (isGzip)
                    {
                        await using var gzip = new GZipStream(counting, CompressionMode.Decompress);
                        await gzip.CopyToAsync(file);
                    }
                    else
                    {
                        await counting.CopyToAsync(file);
                    }
                }

                if (!isGzip && total > 0 && counting.BytesRead < total)
                    throw new IOException($"下载中断：预期 {total:N0} 字节，实际仅接收 {counting.BytesRead:N0} 字节。");

                var list = await ParseFileAsync<TItem>(tempFile);
                if (list == null) throw new IOException("下载内容解析为空。");

                // 解析成功才转正为缓存文件，避免把损坏内容缓存下来
                File.Move(tempFile, cacheFile, overwrite: true);
                _progress.Report(counting.BytesRead, counting.BytesRead);
                _progress.AddLine($"💾 已缓存到本地：{Path.GetFileName(cacheFile)}（{DateTime.Now:yyyy-MM-dd}）");
                return list;
            }
            catch (Exception ex)
            {
                last = ex;
                TryDelete(tempFile);
                _logger.LogWarning(ex, "数据源 {Host} 下载失败，尝试下一个", host);
            }
        }
        throw new IOException($"所有数据源均下载失败（共 {MIRROR_TEMPLATES.Length} 个）：{last?.Message}", last);
    }

    /// <summary>
    /// 只读计数流：统计累计读取字节并回调上报；每次读取带 ReadIdleTimeout，
    /// 连续无新数据即抛 TimeoutException（配合 HttpClient 无限整体超时，实现“慢但在走不掐断”）。
    /// </summary>
    private sealed class ProgressReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly Action<long> _onRead;

        public long BytesRead { get; private set; }

        public ProgressReadStream(Stream inner, Action<long> onRead)
        {
            _inner = inner;
            _onRead = onRead;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            using var idle = CancellationTokenSource.CreateLinkedTokenSource(ct);
            idle.CancelAfter(ReadIdleTimeout);
            int n;
            try
            {
                n = await _inner.ReadAsync(buffer, idle.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"下载超时：连续 {ReadIdleTimeout.TotalSeconds:0} 秒未收到新数据（已接收 {BytesRead / 1024.0 / 1024.0:0.0} MB）。");
            }
            if (n > 0)
            {
                BytesRead += n;
                _onRead(BytesRead);
            }
            return n;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var n = _inner.Read(buffer, offset, count);
            if (n > 0)
            {
                BytesRead += n;
                _onRead(BytesRead);
            }
            return n;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => BytesRead; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>从（缓存的）JSON 文件解析列表</summary>
    private static async Task<List<TItem>?> ParseFileAsync<TItem>(string file) where TItem : class
    {
        await using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, useAsync: true);
        return await JsonSerializer.DeserializeAsync<List<TItem>>(fs, _webJson);
    }

    private static void TryDelete(string file)
    {
        try { if (File.Exists(file)) File.Delete(file); } catch { /* 删除失败不影响主流程 */ }
    }

    /// <summary>从数据库加载已有区域的全路径字典（Level 1-3）</summary>
    private async Task<Dictionary<string, string>> BuildPathCacheAsync()
    {
        var all = await _regionService.GetAllAsync();
        return all.Where(r => r.Level <= 3 && !string.IsNullOrEmpty(r.FullPath))
                  .DistinctBy(r => r.Code)
                  .ToDictionary(r => r.Code, r => r.FullPath!);
    }

    private static string FormatDuration(long ms)
    {
        if (ms < 1000) return $"{ms} ms";
        return $"{(ms / 1000.0):0.#} s";
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
