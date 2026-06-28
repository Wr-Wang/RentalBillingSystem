using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

public interface IHolidayCalendarService
{
    Task<List<HolidayCalendarDto>> GetByYearAsync(int year, CancellationToken ct = default);
    Task<HolidayCalendarDto> CreateAsync(CreateHolidayCalendarRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateHolidayCalendarRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ImportResult> ImportYearDataAsync(int year, CancellationToken ct = default);
}
