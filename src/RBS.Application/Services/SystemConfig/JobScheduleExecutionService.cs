using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
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

        // 删除该任务下所有未来排期（重新生成）
        var all = await _uow.JobScheduleExecutions.GetAllAsync(ct);
        var future = all.Where(e => e.JobScheduleId == jobScheduleId && e.TargetDate > ChinaTime.Now && !e.IsCustom).ToList();
        foreach (var e in future)
            await _uow.JobScheduleExecutions.DeleteAsync(e, ct);

        var created = new List<JobScheduleExecution>();
        var now = ChinaTime.Now;

        // 判断起始月/日（当月可执行则从当月开始）
        int startMonth, startYear = now.Year;
        if (job.ScheduleType == "Daily")
        {
            var todayRun = new DateTime(now.Year, now.Month, now.Day, job.Hour, job.Minute, 0);
            if (now > todayRun)
            {
                var tom = now.AddDays(1);
                startYear = tom.Year; startMonth = tom.Month;
            }
            else
            {
                startMonth = now.Month;
            }
        }
        else
        {
            var execDay = Math.Min(job.DayOfMonth ?? 1, DateTime.DaysInMonth(now.Year, now.Month));
            var thisMonthRun = new DateTime(now.Year, now.Month, execDay, job.Hour, job.Minute, 0);
            if (now > thisMonthRun)
            {
                var next = now.AddMonths(1);
                startYear = next.Year; startMonth = next.Month;
            }
            else
            {
                startMonth = now.Month;
            }
        }

        // 判断结束月/年（≤10月仅当年，>10月含次年）
        int endMonth = 12, endYear = startYear;
        if (startMonth > 11)
            endYear = startYear + 1;

        if (job.ScheduleType == "Daily")
        {
            var d = new DateTime(startYear, startMonth, 1, job.Hour, job.Minute, 0);
            // 调整到正确的起始日
            if (startMonth == now.Month && startYear == now.Year)
            {
                // 从当天开始
                d = now <= new DateTime(now.Year, now.Month, now.Day, job.Hour, job.Minute, 0)
                    ? new DateTime(now.Year, now.Month, now.Day, job.Hour, job.Minute, 0)
                    : new DateTime(now.Year, now.Month, now.Day, job.Hour, job.Minute, 0).AddDays(1);
            }
            var end = new DateTime(endYear, endMonth, 31, 23, 59, 59);
            while (d <= end)
            {
                var exec = new JobScheduleExecution(jobScheduleId, job.CompanyId, d, d, d.ToString("yyyy-MM"), false);
                await _uow.JobScheduleExecutions.AddAsync(exec, ct);
                created.Add(exec);
                d = d.AddDays(1);
            }
        }
        else
        {
            // 仅 BillJob 需加载节假日数据用于调整
            var billJobHolidays = new HashSet<DateOnly>();
            var billJobMakeup = new HashSet<DateOnly>();
            bool isBillJob = job.JobName == "BillJob";
            if (isBillJob)
            {
                // 加载当年+次年节假日数据
                var hols = await _uow.HolidayCalendars.GetAllAsync(ct);
                foreach (var h in hols)
                {
                    if (h.HolidayDate.Year >= startYear && h.HolidayDate.Year <= endYear)
                    {
                        if (h.IsWorkingDay)
                            billJobMakeup.Add(h.HolidayDate);  // 调休上班
                        else
                            billJobHolidays.Add(h.HolidayDate);  // 放假
                    }
                }
            }

            for (int y = startYear; y <= endYear; y++)
            {
                int mStart = (y == startYear) ? startMonth : 1;
                int mEnd = (y == endYear) ? endMonth : 12;
                for (int m = mStart; m <= mEnd; m++)
                {
                    var day = Math.Min(job.DayOfMonth ?? 1, DateTime.DaysInMonth(y, m));
                    var d = new DateTime(y, m, day, job.Hour, job.Minute, 0);
                    var monthKey = d.ToString("yyyy-MM");
                    var originalDate = d;

                    // 仅 BillJob 逢周末/节假日调整至最近工作日
                    var adjusted = isBillJob ? AdjustToWorkingDay(d, billJobHolidays, billJobMakeup) : d;
                    if (adjusted != d)
                    {
                        var reason = $"{d:yyyy-MM-dd}逢周末/节假日，调整至{adjusted:yyyy-MM-dd}";
                        var exec = new JobScheduleExecution(jobScheduleId, job.CompanyId, adjusted, originalDate, monthKey, false);
                        exec.Update(adjusted, "Pending", reason);
                        await _uow.JobScheduleExecutions.AddAsync(exec, ct);
                        created.Add(exec);
                    }
                    else
                    {
                        var exec = new JobScheduleExecution(jobScheduleId, job.CompanyId, d, d, monthKey, false);
                        await _uow.JobScheduleExecutions.AddAsync(exec, ct);
                        created.Add(exec);
                    }
                }
            }
        }

        await _uow.CommitAsync(ct);
        return created.OrderBy(e => e.TargetDate).Select(Map).ToList();
    }

    public async Task DeleteFutureAsync(Guid jobScheduleId, CancellationToken ct = default)
    {
        var all = await _uow.JobScheduleExecutions.GetAllAsync(ct);
        var future = all.Where(e => e.JobScheduleId == jobScheduleId && e.TargetDate > ChinaTime.Now && !e.IsCustom).ToList();
        foreach (var e in future)
            await _uow.JobScheduleExecutions.DeleteAsync(e, ct);
        await _uow.CommitAsync(ct);
    }


    /// <summary>调整日期到最近工作日（向前或向后）</summary>
    private DateTime AdjustToWorkingDay(DateTime date, HashSet<DateOnly> holidays, HashSet<DateOnly> makeupDays)
    {
        var d = DateOnly.FromDateTime(date);
        if (IsWorkingDay(d, holidays, makeupDays)) return date;

        var forward = d.AddDays(1);
        while (!IsWorkingDay(forward, holidays, makeupDays)) forward = forward.AddDays(1);

        var backward = d.AddDays(-1);
        while (!IsWorkingDay(backward, holidays, makeupDays)) backward = backward.AddDays(-1);

        var fwdDays = forward.DayNumber - d.DayNumber;
        var bwdDays = d.DayNumber - backward.DayNumber;
        var nearest = fwdDays <= bwdDays ? forward : backward;

        return new DateTime(nearest.Year, nearest.Month, nearest.Day, date.Hour, date.Minute, date.Second);
    }

    /// <summary>判断是否为工作日（含节假日+调休判断）</summary>
    private bool IsWorkingDay(DateOnly date, HashSet<DateOnly> holidays, HashSet<DateOnly> makeupDays)
    {
        // 调休上班日（周末上班）→ 工作日
        if (makeupDays.Contains(date))
            return true;
        // 法定节假日 → 非工作日
        if (holidays.Contains(date))
            return false;
        // 周六日 → 非工作日
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return false;
        return true;
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
