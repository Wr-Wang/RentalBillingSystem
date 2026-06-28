namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

public class LateFeeConfig : AuditableEntity, IHasCompany
{
    public decimal DailyRate { get; private set; }
    public int GraceDays { get; private set; }
    public decimal? MaxRate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }
    private LateFeeConfig() { }
    public LateFeeConfig(decimal dailyRate, int graceDays, Guid companyId)
    { DailyRate = dailyRate; GraceDays = graceDays; CompanyId = companyId; }
}
