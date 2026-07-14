namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 角色与菜单的关联实体（关联表）
/// 在 DDD 中属于关联实体（AssociationEntity），联结 Role 实体和 Menu 实体，
/// 构成角色-菜单的多对多权限关系
/// </summary>
public class RoleMenu : AssociationEntity
{
    /// <summary>
    /// 角色标识，关联到 Role 实体
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// 菜单标识，关联到 Menu 实体，表示该角色拥有的菜单访问权限
    /// </summary>
    public Guid MenuId { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private RoleMenu() { }

    /// <summary>
    /// 创建角色-菜单关联，授予角色对指定菜单的访问权限
    /// </summary>
    /// <param name="roleId">角色标识</param>
    /// <param name="menuId">菜单标识</param>
    public RoleMenu(Guid roleId, Guid menuId) { RoleId = roleId; MenuId = menuId; }

    /// <summary>
    /// 导航属性：关联的菜单实体（由 EF Core 延迟加载或显式加载填充）
    /// </summary>
    public Menu? Menu { get; private set; }

    /// <summary>
    /// 导航属性：关联的角色实体（由 EF Core 延迟加载或显式加载填充）
    /// </summary>
    public Role? Role { get; private set; }
}
