using RBS.Application.DTOs.Contract;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 租客应用服务接口 — 提供租客的增删改查与唯一性校验能力
/// </summary>
public interface ITenantAppService
{
    /// <summary>
    /// 分页查询租客列表
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="keyword">搜索关键词（姓名/手机号/身份证号）</param>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>分页结果</returns>
    Task<PagedResult<TenantDto>> GetPagedAsync(Guid companyId, string? keyword, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// 根据 ID 获取租客详情
    /// </summary>
    Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 创建新租客
    /// </summary>
    Task<TenantDto> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// 更新租客信息
    /// </summary>
    Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// 删除租客
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 校验手机号是否唯一
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="phone">手机号</param>
    /// <param name="excludeId">需要排除的租客 ID（更新时使用）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>是否唯一</returns>
    Task<bool> IsPhoneUniqueAsync(Guid companyId, string phone, Guid? excludeId, CancellationToken ct = default);

    /// <summary>
    /// 校验身份证号是否唯一
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="idCard">身份证号</param>
    /// <param name="excludeId">需要排除的租客 ID（更新时使用）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>是否唯一</returns>
    Task<bool> IsIdCardUniqueAsync(Guid companyId, string idCard, Guid? excludeId, CancellationToken ct = default);
}
