using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Region;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.Services.Region;

/// <summary>
/// 行政区划本地服务 — 读写 Regions 缓存表
/// 提供前端级联选择和后台管理所需的全部 CRUD
/// </summary>
public class RegionService : IRegionService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public RegionService(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    public async Task<List<RegionDto>> GetProvincesAsync()
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<RegionDto>(_sql.Get("Common.Select.Region.Province"));
        var list = rows.AsList();
        foreach (var r in list) r.HasChildren = true;
        return list;
    }

    public async Task<List<RegionDto>> GetChildrenAsync(string parentCode)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<RegionDto>(
            _sql.Get("Common.Select.Region.ByParentCode"), new { ParentCode = parentCode })).AsList();
    }

    public async Task<RegionDto?> GetByCodeAsync(string code)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<RegionDto>(
            _sql.Get("Common.Select.Region.ByCode"), new { Code = code });
    }

    public async Task<RegionDto?> GetByCodeAndLevelAsync(string code, int level)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<RegionDto>(
            _sql.Get("Common.Select.Region.ByCodeAndLevel"), new { Code = code, Level = level });
    }

    public async Task<List<RegionDto>> SearchAsync(string? keyword)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<RegionDto>(
            _sql.Get("Common.Select.Region.Search"), new { Keyword = keyword })).AsList();
    }

    public async Task<List<RegionDto>> GetAllAsync()
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<RegionDto>(_sql.Get("Common.Select.Region.All"))).AsList();
    }

    public async Task UpsertAsync(RegionDto region)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var exists = await conn.ExecuteScalarAsync<int>(
            _sql.Get("Common.Select.Region.ExistsByCode"), new { Code = region.Code });
        var now = RBS.Core.Common.ChinaTime.Now;
        if (exists > 0)
        {
            var existing = await conn.QuerySingleAsync(
                _sql.Get("Common.Select.Region.ByCode"), new { Code = region.Code });
            await conn.ExecuteAsync(_sql.Get("Common.Update.Region.Default"),
                new { Id = (Guid)existing.Id, Name = region.Name, SortOrder = region.SortOrder,
                    UpdatedBy = Guid.Empty, UpdatedAt = now, UpdatedIp = (string?)null, UpdatedHostname = (string?)null });
        }
        else
        {
            await conn.ExecuteAsync(_sql.Get("Common.Insert.Region.Default"),
                new { Id = Guid.NewGuid(), region.Code, region.Name, region.ParentCode,
                    region.Level, region.FullPath, region.SortOrder,
                    CreatedBy = Guid.Empty, CreatedAt = now, CreatedIp = (string?)null, CreatedHostname = (string?)null });
        }
    }

    public async Task DeleteAsync(string code)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(_sql.Get("Common.Delete.Region.ByParentCode"), new { ParentCode = code }, tx);
        await conn.ExecuteAsync(_sql.Get("Common.Delete.Region.ByCode"), new { Code = code }, tx);
        tx.Commit();
    }

    public async Task DeleteByLevelAsync(int level)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Common.Delete.Region.ByLevelFrom"),
            new { Level = level }, commandTimeout: 120);
    }

    public async Task<bool> ExistsAsync(string code)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.ExecuteScalarAsync<int>(
            _sql.Get("Common.Select.Region.ExistsByCode"), new { Code = code }) > 0;
    }

    public async Task<int> BatchUpsertAsync(List<RegionDto> regions)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        int count = 0;
        foreach (var r in regions)
        {
            var exists = await conn.ExecuteScalarAsync<int>(
                _sql.Get("Common.Select.Region.Exists"), new { Code = r.Code, Level = r.Level }, tx);
            var batchNow = RBS.Core.Common.ChinaTime.Now;
            if (exists > 0)
            {
                var existing = await conn.QuerySingleAsync(
                    _sql.Get("Common.Select.Region.ByCodeAndLevel"), new { Code = r.Code, Level = r.Level }, tx);
                await conn.ExecuteAsync(_sql.Get("Common.Update.Region.Default"),
                    new { Id = (Guid)existing.Id, Name = r.Name, SortOrder = r.SortOrder,
                        UpdatedBy = Guid.Empty, UpdatedAt = batchNow, UpdatedIp = (string?)null, UpdatedHostname = (string?)null }, tx);
            }
            else
            {
                await conn.ExecuteAsync(_sql.Get("Common.Insert.Region.Default"),
                    new { Id = Guid.NewGuid(), r.Code, r.Name, r.ParentCode,
                        r.Level, r.FullPath, r.SortOrder,
                        CreatedBy = Guid.Empty, CreatedAt = batchNow, CreatedIp = (string?)null, CreatedHostname = (string?)null }, tx);
            }
            count++;
        }
        tx.Commit();
        return count;
    }
}
