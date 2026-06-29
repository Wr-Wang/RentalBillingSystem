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
    private Guid CompanyId => _tenant.DefaultCompanyId;

    public async Task<List<JobScheduleDto>> GetJobsAsync(CancellationToken ct = default)
        => (await _uow.JobSchedules.GetAllAsync(ct)).Select(Map).ToList();

    public async Task<JobScheduleDto> CreateAsync(CreateJobScheduleRequest request, CancellationToken ct = default)
    {
        var exists = (await _uow.JobSchedules.GetAllAsync(ct))
            .Any(j => j.CompanyId == CompanyId && j.JobName == request.JobName);
        if (exists)
            throw new InvalidOperationException("同公司下已存在同名任务");

        var entity = new JobSchedule(request.JobName, request.ScheduleType,
            request.Hour, request.Minute, CompanyId, request.DayOfMonth);
        entity.SetDescription(request.Description);
        entity.SetTemplateCode(request.TemplateCode);
        await _uow.JobSchedules.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return Map(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateJobScheduleRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.JobSchedules.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("任务不存在");
        if (entity.CompanyId != CompanyId)
            throw new UnauthorizedAccessException("无权操作其他公司的任务");

        if (request.JobName != null && request.JobName != entity.JobName)
        {
            var exists = (await _uow.JobSchedules.GetAllAsync(ct))
                .Any(j => j.CompanyId == CompanyId && j.JobName == request.JobName && j.Id != id);
            if (exists)
                throw new InvalidOperationException("同公司下已存在同名任务");
            entity.SetJobName(request.JobName);
        }

        if (request.ScheduleType != null)
            entity.SetSchedule(request.ScheduleType, request.Hour ?? entity.Hour,
                request.Minute ?? entity.Minute, request.DayOfMonth);
        if (request.Description != null) entity.SetDescription(request.Description);
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) entity.Activate(); else entity.Deactivate();
        }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.JobSchedules.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("任务不存在");
        if (entity.CompanyId != CompanyId)
            throw new UnauthorizedAccessException("无权操作其他公司的任务");
        await _uow.JobSchedules.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    internal static JobScheduleDto Map(JobSchedule j) => new()
    {
        Id = j.Id,
        JobName = j.JobName,
        ScheduleType = j.ScheduleType,
        Hour = j.Hour,
        Minute = j.Minute,
        DayOfMonth = j.DayOfMonth,
        IsActive = j.IsActive,
        Description = j.Description,
        TemplateCode = j.TemplateCode,
        LastRunAt = j.LastRunAt,
        LastRunStatus = j.LastRunStatus
    };
}
