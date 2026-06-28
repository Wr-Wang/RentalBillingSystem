namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

public class Role : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Role() { }

    /// <summary>领域构造函数</summary>
    public Role(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("角色名称不能为空", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("角色编码不能为空", nameof(code));
        Name = name;
        Code = code;
        IsActive = true;
    }

    // ===== 属性设置 =====
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("角色名称不能为空", nameof(name));
        Name = name;
    }
    public void SetCode(string code) => Code = code;
    public void SetDescription(string? description) => Description = description;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public ICollection<RoleMenu> RoleMenus { get; private set; } = new List<RoleMenu>();
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
}
