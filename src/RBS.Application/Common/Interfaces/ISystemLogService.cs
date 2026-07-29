using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 系统日志查询与管理服务
/// </summary>
public interface ISystemLogService
{
    /// <summary>分页查询系统日志</summary>
    Task<PagedResult<SystemLogDto>> GetListAsync(
        int page, int pageSize, string? level,
        DateTime? startDate, DateTime? endDate,
        CancellationToken ct = default);

    /// <summary>获取单条日志详情</summary>
    Task<SystemLogDto?> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>删除指定日志</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>清空所有日志</summary>
    Task ClearAllAsync(CancellationToken ct = default);
}
