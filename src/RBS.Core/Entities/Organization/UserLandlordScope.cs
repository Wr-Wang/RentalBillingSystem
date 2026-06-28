namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

public class UserLandlordScope : AssociationEntity
{
    public Guid UserId { get; private set; }
    public Guid LandlordId { get; private set; }

    private UserLandlordScope() { }
    public UserLandlordScope(Guid userId, Guid landlordId) { UserId = userId; LandlordId = landlordId; }
}
