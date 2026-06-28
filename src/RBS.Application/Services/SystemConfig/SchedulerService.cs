using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class SchedulerService : ISchedulerService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public SchedulerService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }
    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<JobScheduleDto>> GetJobsAsync(CancellationToken ct = default)
        => (await _uow.JobSchedules.GetAllAsync(ct)).Select(j => new JobScheduleDto
        { Id = j.Id, JobName = j.JobName, CronExpression = j.CronExpression, IsActive = j.IsActive, Description = j.Description }).ToList();

    public async Task<JobScheduleDto> CreateAsync(CreateJobScheduleRequest request, CancellationToken ct = default)
    {
        var entity = new JobSchedule(request.JobName, request.CronExpression, CompanyId);
        entity.SetDescription(request.Description);
        await _uow.JobSchedules.AddAsync(entity, ct); await _uow.CommitAsync(ct);
        return new JobScheduleDto { Id = entity.Id, JobName = entity.JobName, CronExpression = entity.CronExpression, IsActive = entity.IsActive, Description = entity.Description };
    }

    public async Task UpdateAsync(Guid id, UpdateJobScheduleRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.JobSchedules.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("任务不存在");
        if (request.CronExpression != null) entity.SetCron(request.CronExpression);
        if (request.Description != null) entity.SetDescription(request.Description);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) entity.Activate(); else entity.Deactivate(); }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.JobSchedules.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("任务不存在");
        await _uow.JobSchedules.DeleteAsync(entity, ct); await _uow.CommitAsync(ct);
    }
}
