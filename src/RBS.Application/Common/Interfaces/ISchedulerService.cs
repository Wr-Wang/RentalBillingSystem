using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 调度任务定义服务
/// </summary>
public interface ISchedulerService
{
    Task<List<JobScheduleDto>> GetJobsAsync(CancellationToken ct = default);
    Task<JobScheduleDto> CreateAsync(CreateJobScheduleRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateJobScheduleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IJobTemplateService
{
    Task<List<JobTemplateDto>> GetAllAsync(CancellationToken ct = default);
}

public interface IJobScheduleExecutionService
{
    Task<List<ExecutionDto>> GetExecutionsAsync(Guid jobScheduleId, int months, CancellationToken ct = default);
    Task<ExecutionDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExecutionDto> CreateAsync(Guid jobScheduleId, CreateExecutionRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateExecutionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<ExecutionDto>> GenerateAsync(Guid jobScheduleId, CancellationToken ct = default);
}
