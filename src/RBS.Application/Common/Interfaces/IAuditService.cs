using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 审计日志查询服务
/// </summary>
public interface IAuditService
{
    /// <summary>分页查询审计历史</summary>
    Task<PagedResult<AuditEntryDto>> GetHistoryAsync(AuditQuery query, CancellationToken ct = default);

    /// <summary>版本对比</summary>
    Task<List<AuditCompareDto>> CompareAsync(string tableName, string recordId, int v1, int v2, CancellationToken ct = default);

    /// <summary>审计统计</summary>
    Task<AuditStatsDto> GetStatsAsync(CancellationToken ct = default);
}
