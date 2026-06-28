namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

public class RoomPricingStandard : AuditableEntity, IHasCompany
{
    public Guid RoomTypeId { get; private set; }
    public Guid FloorLevelBandId { get; private set; }
    public decimal RentAmount { get; private set; }
    public Guid CompanyId { get; private set; }
    private RoomPricingStandard() { }
    public RoomPricingStandard(Guid roomTypeId, Guid floorLevelBandId, decimal rentAmount, Guid companyId)
    { RoomTypeId = roomTypeId; FloorLevelBandId = floorLevelBandId; RentAmount = rentAmount; CompanyId = companyId; }
}
