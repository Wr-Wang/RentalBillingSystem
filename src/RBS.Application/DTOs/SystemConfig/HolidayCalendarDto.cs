namespace RBS.Application.DTOs.SystemConfig;

public class HolidayCalendarDto
{
    public Guid Id { get; set; }
    public DateOnly HolidayDate { get; set; }
    public string? Name { get; set; }
    public bool IsWorkingDay { get; set; }
    public Guid CompanyId { get; set; }
    /// <summary>节日类型显示文字</summary>
    public string TypeText => IsWorkingDay ? "调休上班" : "放假";
    /// <summary>日期显示</summary>
    public string DateDisplay => HolidayDate.ToString("yyyy-MM-dd (ddd)");
}

public class CreateHolidayCalendarRequest
{
    public DateOnly HolidayDate { get; set; }
    public string? Name { get; set; }
    public bool IsWorkingDay { get; set; }
    public Guid CompanyId { get; set; }
}

public class UpdateHolidayCalendarRequest
{
    public DateOnly? HolidayDate { get; set; }
    public string? Name { get; set; }
    public bool? IsWorkingDay { get; set; }
}

public class ImportResult
{
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<HolidayCalendarDto> Imported { get; set; } = new();
    public List<HolidayCalendarDto> Skipped { get; set; } = new();
}
