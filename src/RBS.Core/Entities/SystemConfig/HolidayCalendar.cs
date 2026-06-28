namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 节假日/调休配置
/// </summary>
public class HolidayCalendar : AuditableEntity, IHasCompany
{
    public DateOnly HolidayDate { get; private set; }
    public string? Name { get; private set; }
    /// <summary>false=放假, true=调休上班</summary>
    public bool IsWorkingDay { get; private set; }
    public Guid CompanyId { get; private set; }

    private HolidayCalendar() { }

    public HolidayCalendar(DateOnly holidayDate, string? name, bool isWorkingDay, Guid companyId)
    {
        HolidayDate = holidayDate;
        Name = name;
        IsWorkingDay = isWorkingDay;
        CompanyId = companyId;
    }

    public void SetName(string? name) => Name = name;
    public void SetIsWorkingDay(bool isWorkingDay) => IsWorkingDay = isWorkingDay;
}
