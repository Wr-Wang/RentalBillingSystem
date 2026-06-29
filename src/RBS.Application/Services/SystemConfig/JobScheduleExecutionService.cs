using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class JobScheduleExecutionService : IJobScheduleExecutionService
{
    private readonly IUnitOfWork _uow;
    public JobScheduleExecutionService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ExecutionDto>> GetExecutionsAsync(Guid jobScheduleId, int months, CancellationToken ct = default)
    {
        var list = await _uow.JobScheduleExecutions.GetAllAsync(ct);
        var result = list
            .Where(e => e.JobScheduleId == jobScheduleId)
            .OrderBy(e => e.TargetDate)
            .Select(Map)
            .ToList();

        if (result.Count == 0)
            result = await GenerateAsync(jobScheduleId, ct);

        return result;
    }

    public async Task<ExecutionDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.JobScheduleExecutions.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("排期不存在");
        return Map(entity);
    }

    public async Task<ExecutionDto> CreateAsync(Guid jobScheduleId, CreateExecutionRequest request, CancellationToken ct = default)
    {
        var job = await _uow.JobSchedules.GetByIdAsync(jobScheduleId, ct)
            ?? throw new KeyNotFoundException("任务不存在");

        var month = request.TargetDate.ToString("yyyy-MM");

        var exists = (await _uow.JobScheduleExecutions.GetAllAsync(ct))
            .Any(e => e.JobScheduleId == jobScheduleId
                   && e.TargetDate == request.TargetDate
                   && e.Reason == request.Reason);
        if (exists)
            throw new InvalidOperationException("该排期已存在，请勿重复添加");

        var execution = new JobScheduleExecution(
            jobScheduleId, job.CompanyId,
            request.TargetDate, null, month, isCustom: true);

        await _uow.JobScheduleExecutions.AddAsync(execution, ct);
        await _uow.CommitAsync(ct);
        return Map(execution);
    }

    public async Task UpdateAsync(Guid id, UpdateExecutionRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.JobScheduleExecutions.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("排期不存在");

        entity.Update(
            request.TargetDate ?? entity.TargetDate,
            request.Status ?? entity.Status,
            request.Reason ?? entity.Reason);

        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.JobScheduleExecutions.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("排期不存在");
        await _uow.JobScheduleExecutions.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    public async Task<List<ExecutionDto>> GenerateAsync(Guid jobScheduleId, CancellationToken ct = default)
    {
        var job = await _uow.JobSchedules.GetByIdAsync(jobScheduleId, ct)
            ?? throw new KeyNotFoundException("任务不存在");

        // 删除该任务下所有未来排期
        var existing = (await _uow.JobScheduleExecutions.GetAllAsync(ct))
            .Where(e => e.JobScheduleId == jobScheduleId && e.TargetDate > DateTime.UtcNow)
            .ToList();

        foreach (var e in existing)
            await _uow.JobScheduleExecutions.DeleteAsync(e, ct);

        // 生成未来 N 个月排期
        var created = new List<JobScheduleExecution>();
        var now = DateTime.UtcNow;

        for (int i = 1; i <= 6; i++)
        {
            var targetMonth = now.AddMonths(i);
            var targetDate = job.ScheduleType == "Daily"
                ? new DateTime(targetMonth.Year, targetMonth.Month, 1, job.Hour, job.Minute, 0)
                : BuildMonthlyDate(targetMonth.Year, targetMonth.Month,
                    job.DayOfMonth ?? 1, job.Hour, job.Minute);

            var month = targetDate.ToString("yyyy-MM");
            var execution = new JobScheduleExecution(
                jobScheduleId, job.CompanyId,
                targetDate, targetDate, month, isCustom: false);

            await _uow.JobScheduleExecutions.AddAsync(execution, ct);
            created.Add(execution);
        }

        // EF Core SaveChanges 自动在事务内执行 Delete + Insert
        await _uow.CommitAsync(ct);
        return created.OrderBy(e => e.TargetDate).Select(Map).ToList();
    }

    private static DateTime BuildMonthlyDate(int year, int month, int day, int hour, int minute)
    {
        var maxDay = DateTime.DaysInMonth(year, month);
        return new DateTime(year, month, Math.Min(day, maxDay), hour, minute, 0);
    }

    internal static ExecutionDto Map(JobScheduleExecution e) => new()
    {
        Id = e.Id,
        JobScheduleId = e.JobScheduleId,
        Month = e.Month,
        TargetDate = e.TargetDate,
        OriginalDate = e.OriginalDate,
        Status = e.Status,
        Reason = e.Reason,
        IsAdjusted = e.IsAdjusted,
        IsCustom = e.IsCustom
    };
}
