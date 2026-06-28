namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

public class UserRole : AssociationEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    private UserRole() { }
    public UserRole(Guid userId, Guid roleId) { UserId = userId; RoleId = roleId; }

    public Role? Role { get; private set; }
}
