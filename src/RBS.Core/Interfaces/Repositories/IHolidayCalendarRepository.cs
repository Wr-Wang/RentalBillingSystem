namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.SystemConfig;

public interface IHolidayCalendarRepository : IRepository<HolidayCalendar>
{
    Task<List<HolidayCalendar>> GetByYearAsync(Guid companyId, int year, CancellationToken ct = default);
    Task<HolidayCalendar?> GetByDateAsync(Guid companyId, DateOnly date, CancellationToken ct = default);
}
