using RBS.Application.DTOs.Property;
namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 房源管理应用服务接口 — 提供房源（房屋单元）的增删改查与统计能力
/// </summary>
public interface IHousingUnitService
{
    /// <summary>
    /// 获取房源列表（支持按楼宇名称、关键词、状态筛选）
    /// </summary>
    /// <param name="buildingName">楼宇名称筛选</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="status">房源状态筛选</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>房源响应列表</returns>
    Task<List<HousingUnitResponse>> GetListAsync(string? buildingName = null, string? keyword = null, string? status = null, CancellationToken ct = default);

    /// <summary>
    /// 根据 ID 获取房源详情
    /// </summary>
    /// <param name="id">房源 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>房源响应 DTO，不存在则返回 null</returns>
    Task<HousingUnitResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 创建新房源
    /// </summary>
    /// <param name="request">创建房源请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>创建的房源 DTO</returns>
    Task<HousingUnitResponse> CreateAsync(CreateHousingUnitRequest request, CancellationToken ct = default);

    /// <summary>
    /// 更新房源信息
    /// </summary>
    /// <param name="id">房源 ID</param>
    /// <param name="request">更新房源请求</param>
    /// <param name="ct">取消令牌</param>
    Task UpdateAsync(Guid id, UpdateHousingUnitRequest request, CancellationToken ct = default);

    /// <summary>
    /// 删除房源
    /// </summary>
    /// <param name="id">房源 ID</param>
    /// <param name="ct">取消令牌</param>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 获取房源树形结构（按楼宇分组）
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>树形楼宇 DTO 列表</returns>
    Task<List<TreeBuildingDto>> GetTreeAsync(Guid companyId, CancellationToken ct = default);

    /// <summary>
    /// 获取房源统计信息
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>房源统计数据</returns>
    Task<HousingUnitStatsResponse> GetStatsAsync(CancellationToken ct = default);
}
