namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class MeterReading : AuditableEntity
{
    public Guid ContractFeeConfigId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal PreviousReading { get; private set; }
    public decimal CurrentReading { get; private set; }
    public decimal Usage => CurrentReading - PreviousReading;
    public string Status { get; private set; } = "Draft";
    private MeterReading() { }
    public MeterReading(Guid contractFeeConfigId, int year, int month, decimal previous, decimal current)
    { ContractFeeConfigId = contractFeeConfigId; Year = year; Month = month; PreviousReading = previous; CurrentReading = current; }
}
