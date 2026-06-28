namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

public class Role : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Role() { }
    public Role(string name, string code) { Name = name; Code = code; }

    public ICollection<RoleMenu> RoleMenus { get; private set; } = new List<RoleMenu>();
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
}
