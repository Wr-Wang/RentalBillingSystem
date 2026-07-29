using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// API 日志应用服务 — 查询与清理 API 调用日志
/// </summary>
public interface IApiLogService
{
    /// <summary>
    /// API 日志列表查询（默认近 7 天，排除 RequestBody/ResponseBody 大字段）
    /// </summary>
    Task<(List<ApiLogListItemDto> Items, int Total)> GetListAsync(
        int page, int pageSize, string? method, string? path,
        int? statusCode, Guid? userId, DateTime? startDate, DateTime? endDate,
        CancellationToken ct = default);

    /// <summary>API 日志详情（含请求/响应正文）</summary>
    Task<ApiLogDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>删除单条 API 日志</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>按时间范围批量删除 API 日志</summary>
    Task DeleteByRangeAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
}
