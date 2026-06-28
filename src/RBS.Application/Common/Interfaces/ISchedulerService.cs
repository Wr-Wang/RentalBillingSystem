using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

public interface ISchedulerService
{
    Task<List<JobScheduleDto>> GetJobsAsync(CancellationToken ct = default);
    Task<JobScheduleDto> CreateAsync(CreateJobScheduleRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateJobScheduleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
