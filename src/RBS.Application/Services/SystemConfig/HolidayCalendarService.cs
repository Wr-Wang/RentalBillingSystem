using System.Data;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class HolidayCalendarService : IHolidayCalendarService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    private readonly IBulkInserter _bulk;

    public HolidayCalendarService(IUnitOfWork uow, ITenantService tenant, IBulkInserter bulk)
    {
        _uow = uow;
        _tenant = tenant;
        _bulk = bulk;
    }

    private Guid CurrentCompanyId => _tenant.EffectiveCompanyId ?? _tenant.CompanyId ?? Guid.Empty;

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

        if (request.HolidayDate.HasValue && request.HolidayDate.Value != holiday.HolidayDate)
        {
            // 日期变更 → 重建（HolidayCalendar 的 Date 不可变）
            await _uow.HolidayCalendars.DeleteAsync(holiday, ct);
            holiday = new HolidayCalendar(request.HolidayDate.Value, holiday.Name, holiday.IsWorkingDay, holiday.CompanyId);
            await _uow.HolidayCalendars.AddAsync(holiday, ct);
        }
        else
        {
            if (request.Name != null) holiday.SetName(request.Name);
            if (request.IsWorkingDay.HasValue) holiday.SetIsWorkingDay(request.IsWorkingDay.Value);
            await _uow.CommitAsync(ct);
        }
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

        // 加载已有数据用于去重
        var existing = await _uow.HolidayCalendars.GetByYearAsync(companyId, year, ct);
        var existingDates = new HashSet<DateOnly>(existing.Select(h => h.HolidayDate));

        var toInsert = new List<HolidayCalendar>();
        var imported = new List<HolidayCalendarDto>();
        var skipped = new List<HolidayCalendarDto>();

        // 收集待插入数据（先不写库）
        foreach (var (dateStr, raw) in response.Holidays ?? new())
            CollectDate(dateStr, raw, false, companyId, existingDates, toInsert, imported, skipped);

        foreach (var (dateStr, raw) in response.Workdays ?? new())
            CollectDate(dateStr, raw, true, companyId, existingDates, toInsert, imported, skipped);

        // 批量写入
        if (toInsert.Count > 0)
        {
            var dt = BuildHolidayDataTable(toInsert);
            await _bulk.BulkInsertAsync("HolidayCalendars", dt, ct);
        }

        return new ImportResult
        {
            Imported = imported,
            Skipped = skipped,
            ImportedCount = imported.Count,
            SkippedCount = skipped.Count
        };
    }

    private static void CollectDate(string dateStr, string raw, bool isWorkingDay, Guid companyId,
        HashSet<DateOnly> existingDates,
        List<HolidayCalendar> toInsert, List<HolidayCalendarDto> imported, List<HolidayCalendarDto> skipped)
    {
        if (!DateOnly.TryParse(dateStr, out var date)) return;
        var name = raw.Split(',').ElementAtOrDefault(1) ?? raw.Split(',').ElementAtOrDefault(0) ?? "节假日";

        if (existingDates.Contains(date))
        {
            skipped.Add(new HolidayCalendarDto { Id = Guid.Empty, HolidayDate = date, Name = name });
            return;
        }

        var holiday = new HolidayCalendar(date, name, isWorkingDay, companyId);
        toInsert.Add(holiday);
        imported.Add(MapToDto(holiday));
    }

    private static HolidayCalendarDto MapToDto(HolidayCalendar h) => new()
    {
        Id = h.Id,
        HolidayDate = h.HolidayDate,
        Name = h.Name,
        IsWorkingDay = h.IsWorkingDay,
        CompanyId = h.CompanyId
    };

    /// <summary>构建 HolidayCalendar DataTable</summary>
    private static DataTable BuildHolidayDataTable(IReadOnlyList<HolidayCalendar> items)
    {
        var dt = new DataTable("HolidayCalendars");
        dt.Columns.Add("Id", typeof(Guid));
        dt.Columns.Add("HolidayDate", typeof(DateOnly));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("IsWorkingDay", typeof(bool));
        dt.Columns.Add("Year", typeof(int));
        dt.Columns.Add("CompanyId", typeof(Guid));
        dt.Columns.Add("CreatedAt", typeof(DateTime));

        foreach (var h in items)
            dt.Rows.Add(h.Id, h.HolidayDate, h.Name, h.IsWorkingDay, h.HolidayDate.Year, h.CompanyId, ChinaTime.Now);

        return dt;
    }
}

/// <summary>chinese-days CDN 响应模型</summary>
internal class ChineseDaysResponse
{
    [JsonPropertyName("holidays")] public Dictionary<string, string>? Holidays { get; set; }
    [JsonPropertyName("workdays")] public Dictionary<string, string>? Workdays { get; set; }
}

