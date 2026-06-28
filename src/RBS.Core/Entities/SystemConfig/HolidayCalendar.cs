namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class HolidayCalendar : AuditableEntity, IHasLandlord
{
    public DateOnly HolidayDate { get; private set; }
    public string? Name { get; private set; }
    public bool IsWorkingDay { get; private set; }
    public Guid LandlordId { get; private set; }
    private HolidayCalendar() { }
    public HolidayCalendar(DateOnly holidayDate, Guid landlordId) { HolidayDate = holidayDate; LandlordId = landlordId; }
}
