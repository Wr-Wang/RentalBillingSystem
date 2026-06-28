using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class HolidayCalendarRepository : BaseRepository<HolidayCalendar>, IHolidayCalendarRepository
{
    public HolidayCalendarRepository(AppDbContext context) : base(context) { }

    public async Task<List<HolidayCalendar>> GetByYearAsync(Guid companyId, int year, CancellationToken ct = default)
    {
        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        return await _dbSet
            .Where(h => h.CompanyId == companyId && h.HolidayDate >= start && h.HolidayDate <= end)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync(ct);
    }

    public async Task<HolidayCalendar?> GetByDateAsync(Guid companyId, DateOnly date, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.CompanyId == companyId && h.HolidayDate == date, ct);
    }
}
