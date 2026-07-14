using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 调度任务定义服务接口 — 提供定时作业的 CRUD 管理能力
/// </summary>
public interface ISchedulerService
{
    /// <summary>
    /// 获取所有调度任务定义
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>调度任务 DTO 列表</returns>
    Task<List<JobScheduleDto>> GetJobsAsync(CancellationToken ct = default);

    /// <summary>
    /// 创建新的调度任务
    /// </summary>
    /// <param name="request">创建调度任务请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>创建的调度任务 DTO</returns>
    Task<JobScheduleDto> CreateAsync(CreateJobScheduleRequest request, CancellationToken ct = default);

    /// <summary>
    /// 更新调度任务
    /// </summary>
    /// <param name="id">调度任务 ID</param>
    /// <param name="request">更新调度任务请求</param>
    /// <param name="ct">取消令牌</param>
    Task UpdateAsync(Guid id, UpdateJobScheduleRequest request, CancellationToken ct = default);

    /// <summary>
    /// 删除调度任务
    /// </summary>
    /// <param name="id">调度任务 ID</param>
    /// <param name="ct">取消令牌</param>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// 作业模板服务接口 — 提供预定义作业模板的查询能力
/// </summary>
public interface IJobTemplateService
{
    /// <summary>
    /// 获取所有作业模板
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业模板 DTO 列表</returns>
    Task<List<JobTemplateDto>> GetAllAsync(CancellationToken ct = default);
}

/// <summary>
/// 调度执行计划服务接口 — 提供调度任务执行记录的管理能力
/// </summary>
public interface IJobScheduleExecutionService
{
    /// <summary>
    /// 获取指定调度任务最近 N 个月的执行记录
    /// </summary>
    /// <param name="jobScheduleId">调度任务 ID</param>
    /// <param name="months">回溯月数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>执行记录列表</returns>
    Task<List<ExecutionDto>> GetExecutionsAsync(Guid jobScheduleId, int months, CancellationToken ct = default);

    /// <summary>
    /// 根据 ID 获取执行记录详情
    /// </summary>
    Task<ExecutionDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 手动创建执行记录
    /// </summary>
    Task<ExecutionDto> CreateAsync(Guid jobScheduleId, CreateExecutionRequest request, CancellationToken ct = default);

    /// <summary>
    /// 更新执行记录
    /// </summary>
    Task UpdateAsync(Guid id, UpdateExecutionRequest request, CancellationToken ct = default);

    /// <summary>
    /// 删除执行记录
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 自动生成指定调度任务的未来执行记录
    /// </summary>
    Task<List<ExecutionDto>> GenerateAsync(Guid jobScheduleId, CancellationToken ct = default);

    /// <summary>
    /// 删除指定调度任务的所有未来执行记录
    /// </summary>
    Task DeleteFutureAsync(Guid jobScheduleId, CancellationToken ct = default);
}
