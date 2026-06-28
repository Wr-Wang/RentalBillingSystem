namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

public class RoomFeeDefault : AuditableEntity, IHasLandlord
{
    public Guid RoomId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public decimal Amount { get; private set; }
    public Guid LandlordId { get; private set; }
    private RoomFeeDefault() { }
    public RoomFeeDefault(Guid roomId, Guid feeCodeId, decimal amount, Guid landlordId)
    { RoomId = roomId; FeeCodeId = feeCodeId; Amount = amount; LandlordId = landlordId; }
}
