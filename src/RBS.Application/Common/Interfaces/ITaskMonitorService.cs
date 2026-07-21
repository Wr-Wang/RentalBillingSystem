using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 调度执行监控服务接口
/// </summary>
public interface ITaskMonitorService
{
    /// <summary>获取执行总览统计（今日/卡片数据）</summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);

    /// <summary>获取成功率趋势数据</summary>
    Task<List<TrendPointDto>> GetSuccessRateTrendAsync(int days = 30, CancellationToken ct = default);

    /// <summary>获取各任务平均耗时对比</summary>
    Task<List<TaskAvgDurationDto>> GetTaskAvgDurationAsync(int days = 30, CancellationToken ct = default);

    /// <summary>获取失败原因聚合</summary>
    Task<List<FailureAggregationDto>> GetFailureAggregationAsync(int days = 30, CancellationToken ct = default);

    /// <summary>分页查询执行日志（多维度筛选）</summary>
    Task<PagedResult<TaskLogListItemDto>> QueryTaskLogsAsync(TaskLogQuery query, CancellationToken ct = default);

    /// <summary>获取任务的步骤日志（含耗时明细）</summary>
    Task<TaskLogDetailDto> GetTaskLogDetailAsync(Guid taskLogId, CancellationToken ct = default);

    /// <summary>反转预览 — 估算反转影响范围</summary>
    Task<ReversePreviewDto> PreviewReverseAsync(Guid taskLogId, CancellationToken ct = default);

    /// <summary>获取失败合同明细（BillJob）</summary>
    Task<List<FailedContractDto>> GetFailedContractsAsync(Guid taskLogId, CancellationToken ct = default);
}
