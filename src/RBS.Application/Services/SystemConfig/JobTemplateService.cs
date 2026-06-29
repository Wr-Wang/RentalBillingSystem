using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class JobTemplateService : IJobTemplateService
{
    private readonly IUnitOfWork _uow;
    public JobTemplateService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<JobTemplateDto>> GetAllAsync(CancellationToken ct = default)
    {
        var templates = await _uow.JobTemplates.GetAllAsync(ct);
        return templates.Where(t => t.IsActive).OrderBy(t => t.SortOrder).Select(t => new JobTemplateDto
        {
            Id = t.Id,
            Code = t.Code,
            DisplayName = t.DisplayName,
            ShortName = t.ShortName,
            DefaultScheduleType = t.DefaultScheduleType,
            DefaultHour = t.DefaultHour,
            DefaultMinute = t.DefaultMinute,
            DefaultDayOfMonth = t.DefaultDayOfMonth,
            Description = t.Description,
            Icon = t.Icon,
            Category = t.Category
        }).ToList();
    }
}
