namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 用户实体（聚合根）
/// 表示系统中的操作员账号，包含认证信息、权限角色及账户状态管理
/// </summary>
public class User : AggregateRoot
{
    private readonly List<UserRole> _roles = new();

    /// <summary>
    /// 登录用户名（唯一）
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// 密码哈希值
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// 用户显示名称
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// 手机号码
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// 账户是否处于激活状态
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 所属房东标识（为空表示平台级用户）
    /// </summary>
    public Guid? HomeLandlordId { get; private set; }

    /// <summary>
    /// 是否为超级管理员（全局最高权限）
    /// </summary>
    public bool IsSuperAdmin { get; private set; }

    /// <summary>
    /// 用户拥有的角色关联集合（只读）
    /// </summary>
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private User() { }

    /// <summary>
    /// 创建用户实例
    /// </summary>
    /// <param name="username">登录用户名</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="passwordHash">密码哈希值</param>
    /// <exception cref="ArgumentException">当用户名或显示名称为空时抛出</exception>
    public User(string username, string displayName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("用户名不能为空", nameof(username));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("显示名称不能为空", nameof(displayName));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("密码哈希不能为空", nameof(passwordHash));

        Username = username.Trim();
        DisplayName = displayName.Trim();
        PasswordHash = passwordHash;
        IsActive = true;
    }

    /// <summary>
    /// 为用户分配指定角色
    /// </summary>
    /// <param name="roleId">角色标识</param>
    /// <exception cref="InvalidOperationException">当用户已拥有该角色时抛出</exception>
    public void AssignRole(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
            throw new InvalidOperationException($"用户「{DisplayName}」已拥有该角色");

        _roles.Add(new UserRole(Id, roleId));
    }

    /// <summary>
    /// 移除用户的指定角色
    /// </summary>
    /// <param name="roleId">角色标识</param>
    /// <exception cref="InvalidOperationException">当用户未拥有该角色时抛出</exception>
    public void RemoveRole(Guid roleId)
    {
        var userRole = _roles.FirstOrDefault(r => r.RoleId == roleId)
            ?? throw new InvalidOperationException($"用户「{DisplayName}」未拥有该角色，无法移除");

        _roles.Remove(userRole);
    }

    /// <summary>
    /// 判断用户是否拥有指定角色
    /// </summary>
    /// <param name="roleId">角色标识</param>
    public bool HasRole(Guid roleId) => _roles.Any(r => r.RoleId == roleId);

    /// <summary>
    /// 更新用户基本资料
    /// </summary>
    /// <param name="displayName">显示名称</param>
    /// <param name="phone">手机号码（可选）</param>
    /// <param name="email">电子邮箱（可选）</param>
    /// <exception cref="ArgumentException">当显示名称为空时抛出</exception>
    public void UpdateProfile(string displayName, string? phone = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("显示名称不能为空", nameof(displayName));

        DisplayName = displayName.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="newPasswordHash">新密码的哈希值</param>
    /// <exception cref="ArgumentException">当密码哈希为空时抛出</exception>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("密码哈希不能为空", nameof(newPasswordHash));

        if (newPasswordHash == PasswordHash)
            throw new InvalidOperationException("新密码不能与旧密码相同");

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// 激活用户账户
    /// </summary>
    /// <exception cref="InvalidOperationException">当账户已激活时抛出</exception>
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException($"用户「{DisplayName}」当前已是激活状态");

        IsActive = true;
    }

    /// <summary>
    /// 停用用户账户
    /// </summary>
    /// <exception cref="InvalidOperationException">当账户已停用时抛出</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException($"用户「{DisplayName}」当前已是停用状态");

        IsActive = false;
    }

    /// <summary>
    /// 设置为超级管理员
    /// </summary>
    /// <exception cref="InvalidOperationException">当用户已是超级管理员时抛出</exception>
    public void GrantSuperAdmin()
    {
        if (IsSuperAdmin)
            throw new InvalidOperationException($"用户「{DisplayName}」已经是超级管理员");

        IsSuperAdmin = true;
    }

    /// <summary>
    /// 撤销超级管理员权限
    /// </summary>
    /// <exception cref="InvalidOperationException">当用户不是超级管理员时抛出</exception>
    public void RevokeSuperAdmin()
    {
        if (!IsSuperAdmin)
            throw new InvalidOperationException($"用户「{DisplayName}」不是超级管理员");

        IsSuperAdmin = false;
    }

    /// <summary>
    /// 设置所属房东
    /// </summary>
    /// <param name="landlordId">房东标识</param>
    public void SetHomeLandlord(Guid? landlordId)
    {
        HomeLandlordId = landlordId;
    }
}
