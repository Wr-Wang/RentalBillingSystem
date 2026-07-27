using RBS.Application.DTOs.Region;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 行政区划服务 — 提供 Regions 表的本地查询和管理
/// </summary>
public interface IRegionService
{
    /// <summary>获取省份列表（Level=1）</summary>
    Task<List<RegionDto>> GetProvincesAsync();

    /// <summary>根据父级代码获取子级列表</summary>
    Task<List<RegionDto>> GetChildrenAsync(string parentCode);

    /// <summary>根据代码获取区域信息</summary>
    Task<RegionDto?> GetByCodeAsync(string code);

    /// <summary>根据代码和层级获取区域信息</summary>
    Task<RegionDto?> GetByCodeAndLevelAsync(string code, int level);

    /// <summary>搜索区域（按名称或代码模糊匹配）</summary>
    Task<List<RegionDto>> SearchAsync(string? keyword);

    /// <summary>获取所有区域（用于管理后台）</summary>
    Task<List<RegionDto>> GetAllAsync();

    /// <summary>新增或更新区域（同步写入）</summary>
    Task UpsertAsync(RegionDto region);

    /// <summary>删除区域及其子级</summary>
    Task DeleteAsync(string code);

    /// <summary>按层级删除</summary>
    Task DeleteByLevelAsync(int level);

    /// <summary>判断代码是否已存在</summary>
    Task<bool> ExistsAsync(string code);

    /// <summary>批量写入区域（用于 API 同步）</summary>
    Task<int> BatchUpsertAsync(List<RegionDto> regions);
}
