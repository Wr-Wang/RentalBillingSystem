namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class LateFeeConfig : AuditableEntity, IHasLandlord
{
    public decimal DailyRate { get; private set; }
    public int GraceDays { get; private set; }
    public decimal? MaxRate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid LandlordId { get; private set; }
    private LateFeeConfig() { }
    public LateFeeConfig(decimal dailyRate, int graceDays, Guid landlordId)
    { DailyRate = dailyRate; GraceDays = graceDays; LandlordId = landlordId; }
}
