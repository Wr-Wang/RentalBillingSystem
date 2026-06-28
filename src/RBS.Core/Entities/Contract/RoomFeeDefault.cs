namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

public class RoomFeeDefault : AuditableEntity, IHasCompany
{
    public Guid RoomId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public decimal Amount { get; private set; }
    public Guid CompanyId { get; private set; }
    private RoomFeeDefault() { }
    public RoomFeeDefault(Guid roomId, Guid feeCodeId, decimal amount, Guid companyId)
    { RoomId = roomId; FeeCodeId = feeCodeId; Amount = amount; CompanyId = companyId; }
}
