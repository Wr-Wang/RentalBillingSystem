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

    /// <summary>获取所有已配置的审计表清单（供前端动态加载）</summary>
    Task<List<AuditTableInfo>> GetAuditTablesAsync(CancellationToken ct = default);

    /// <summary>回滚到指定版本 — 从 _Audit 表读取版本数据，恢复主表</summary>
    Task<AuditRollbackResult> RollbackAsync(string tableName, string recordId, int versionNo, CancellationToken ct = default);
}

/// <summary>
/// 审计表信息（用于前端下拉列表）
/// </summary>
public class AuditTableInfo
{
    public string TableName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int TotalChanges { get; set; }
}
