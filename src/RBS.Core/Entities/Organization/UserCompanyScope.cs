namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

public class UserCompanyScope : AssociationEntity
{
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }

    private UserCompanyScope() { }
    public UserCompanyScope(Guid userId, Guid companyId) { UserId = userId; CompanyId = companyId; }
}
