namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

public class RoleMenu : AssociationEntity
{
    public Guid RoleId { get; private set; }
    public Guid MenuId { get; private set; }

    private RoleMenu() { }
    public RoleMenu(Guid roleId, Guid menuId) { RoleId = roleId; MenuId = menuId; }

    public Menu? Menu { get; private set; }
    public Role? Role { get; private set; }
}
