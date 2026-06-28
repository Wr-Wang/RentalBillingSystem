namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class HolidayCalendar : AuditableEntity, IHasCompany
{
    public DateOnly HolidayDate { get; private set; }
    public string? Name { get; private set; }
    public bool IsWorkingDay { get; private set; }
    public Guid CompanyId { get; private set; }
    private HolidayCalendar() { }
    public HolidayCalendar(DateOnly holidayDate, Guid companyId) { HolidayDate = holidayDate; CompanyId = companyId; }
}
