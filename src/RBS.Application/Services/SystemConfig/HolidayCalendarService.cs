using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class HolidayCalendarService : IHolidayCalendarService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;

    public HolidayCalendarService(IUnitOfWork uow, ITenantService tenant)
    {
        _uow = uow;
        _tenant = tenant;
    }

    private Guid CurrentCompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<HolidayCalendarDto>> GetByYearAsync(int year, CancellationToken ct = default)
    {
        var items = await _uow.HolidayCalendars.GetByYearAsync(CurrentCompanyId, year, ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<HolidayCalendarDto> CreateAsync(CreateHolidayCalendarRequest request, CancellationToken ct = default)
    {
        var holiday = new HolidayCalendar(request.HolidayDate, request.Name, request.IsWorkingDay, CurrentCompanyId);
        await _uow.HolidayCalendars.AddAsync(holiday, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(holiday);
    }

    public async Task UpdateAsync(Guid id, UpdateHolidayCalendarRequest request, CancellationToken ct = default)
    {
        var holiday = await _uow.HolidayCalendars.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("节假日不存在");
        if (request.HolidayDate.HasValue)
            holiday = new HolidayCalendar(request.HolidayDate.Value, holiday.Name, holiday.IsWorkingDay, holiday.CompanyId);
        if (request.Name != null) holiday.SetName(request.Name);
        if (request.IsWorkingDay.HasValue) holiday.SetIsWorkingDay(request.IsWorkingDay.Value);
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var holiday = await _uow.HolidayCalendars.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("节假日不存在");
        await _uow.HolidayCalendars.DeleteAsync(holiday, ct);
        await _uow.CommitAsync(ct);
    }

    public async Task<ImportResult> ImportYearDataAsync(int year, CancellationToken ct = default)
    {
        var companyId = CurrentCompanyId;
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var url = $"https://cdn.jsdelivr.net/npm/chinese-days/dist/years/{year}.json";
        var response = await http.GetFromJsonAsync<ChineseDaysResponse>(url, ct);

        if (response == null)
            throw new InvalidOperationException("获取节假日数据失败，请检查网络或年份");

        var imported = new List<HolidayCalendarDto>();
        var skipped = new List<HolidayCalendarDto>();

        // 节假日（放假）
        foreach (var (dateStr, raw) in response.Holidays ?? new())
        {
            var ok = await ProcessDate(dateStr, raw, false, companyId, ct, imported, skipped);
            if (!ok) continue;
        }
        // 调休上班
        foreach (var (dateStr, raw) in response.Workdays ?? new())
        {
            var ok = await ProcessDate(dateStr, raw, true, companyId, ct, imported, skipped);
            if (!ok) continue;
        }

        await _uow.CommitAsync(ct);
        return new ImportResult
        {
            Imported = imported,
            Skipped = skipped,
            ImportedCount = imported.Count,
            SkippedCount = skipped.Count
        };
    }

    private async Task<bool> ProcessDate(string dateStr, string raw, bool isWorkingDay, Guid companyId,
        CancellationToken ct, List<HolidayCalendarDto> imported, List<HolidayCalendarDto> skipped)
    {
        if (!DateOnly.TryParse(dateStr, out var date)) return false;
        var name = raw.Split(',').ElementAtOrDefault(1) ?? raw.Split(',').ElementAtOrDefault(0) ?? "节假日";

        var exist = await _uow.HolidayCalendars.GetByDateAsync(companyId, date, ct);
        if (exist != null) { skipped.Add(MapToDto(exist)); return false; }

        var holiday = new HolidayCalendar(date, name, isWorkingDay, companyId);
        await _uow.HolidayCalendars.AddAsync(holiday, ct);
        imported.Add(MapToDto(holiday));
        return true;
    }

    private static HolidayCalendarDto MapToDto(HolidayCalendar h) => new()
    {
        Id = h.Id,
        HolidayDate = h.HolidayDate,
        Name = h.Name,
        IsWorkingDay = h.IsWorkingDay,
        CompanyId = h.CompanyId
    };
}

/// <summary>chinese-days CDN 响应模型</summary>
internal class ChineseDaysResponse
{
    [JsonPropertyName("holidays")] public Dictionary<string, string>? Holidays { get; set; }
    [JsonPropertyName("workdays")] public Dictionary<string, string>? Workdays { get; set; }
}

