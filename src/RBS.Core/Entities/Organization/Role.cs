namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 角色实体 — 权限集合的载体（AuditableEntity）
/// 角色是权限分配的基本单位，一个角色包含多个菜单访问权限，
/// 一个用户可以拥有多个角色，构成灵活的多对多授权模型
/// </summary>
public class Role : AuditableEntity
{
    /// <summary>
    /// 角色名称，如"管理员"、"财务审核人"等业务名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 角色编码（唯一），用于系统内部标识和权限判断，如"Admin"、"FinanceAuditor"
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 角色描述（可选），说明角色的权限范围和适用场景
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 是否启用，true=启用（正常分配使用），false=停用（不可分配）
    /// 默认值为 true
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private Role() { }

    /// <summary>
    /// 创建角色实例。角色名称和编码均为必填项，创建后默认处于启用状态
    /// </summary>
    /// <param name="name">角色名称，不能为空或空白字符</param>
    /// <param name="code">角色编码，不能为空或空白字符，通常使用英文/驼峰命名</param>
    /// <exception cref="ArgumentException">当角色名称或编码为空时抛出</exception>
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

    // ===== 属性设置方法 =====

    /// <summary>重命名角色</summary>
    /// <param name="name">新的角色名称，不能为空或空白字符</param>
    /// <exception cref="ArgumentException">当新名称为空时抛出</exception>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("角色名称不能为空", nameof(name));
        Name = name;
    }

    /// <summary>设置角色编码</summary>
    /// <param name="code">新的角色编码，通常使用英文驼峰命名</param>
    public void SetCode(string code) => Code = code;

    /// <summary>设置角色描述</summary>
    /// <param name="description">描述文本，可设为 null 表示清空</param>
    public void SetDescription(string? description) => Description = description;

    /// <summary>启用角色，恢复角色的可分配使用状态</summary>
    public void Activate() => IsActive = true;

    /// <summary>停用角色，禁止继续分配该角色给用户</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// 角色关联的菜单权限集合（只读）
    /// 用于 EF Core 导航属性，记录该角色可以访问的菜单列表
    /// </summary>
    private readonly List<RoleMenu> _roleMenus = new();

    /// <summary>
    /// 拥有该角色的用户关联集合（只读）
    /// 用于 EF Core 导航属性，记录被分配该角色的所有用户
    /// </summary>
    private readonly List<UserRole> _userRoles = new();

    /// <summary>
    /// 获取角色关联的菜单权限只读集合
    /// </summary>
    public IReadOnlyCollection<RoleMenu> RoleMenus => _roleMenus.AsReadOnly();

    /// <summary>
    /// 获取拥有该角色的用户关联只读集合
    /// </summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    /// <summary>为角色分配单个菜单权限</summary>
    /// <param name="menuId">菜单标识</param>
    public void AssignMenu(Guid menuId) => _roleMenus.Add(new RoleMenu(Id, menuId));

    /// <summary>批量覆盖分配菜单权限（先清空后添加）</summary>
    /// <param name="menuIds">菜单标识集合</param>
    public void AssignMenus(IEnumerable<Guid> menuIds)
    {
        _roleMenus.Clear();
        foreach (var id in menuIds) _roleMenus.Add(new RoleMenu(Id, id));
    }

    /// <summary>清空角色的所有菜单权限</summary>
    public void ClearMenus() => _roleMenus.Clear();

    /// <summary>将角色分配给指定用户</summary>
    /// <param name="userId">用户标识</param>
    public void AssignRole(Guid userId) => _userRoles.Add(new UserRole(userId, Id));
}
