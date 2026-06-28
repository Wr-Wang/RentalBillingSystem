namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

public class RoomPricingStandard : AuditableEntity, IHasLandlord
{
    public Guid RoomTypeId { get; private set; }
    public Guid FloorLevelBandId { get; private set; }
    public decimal RentAmount { get; private set; }
    public Guid LandlordId { get; private set; }
    private RoomPricingStandard() { }
    public RoomPricingStandard(Guid roomTypeId, Guid floorLevelBandId, decimal rentAmount, Guid landlordId)
    { RoomTypeId = roomTypeId; FloorLevelBandId = floorLevelBandId; RentAmount = rentAmount; LandlordId = landlordId; }
}
