namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 用户与角色的关联实体（关联表）
/// 在 DDD 中属于关联实体（AssociationEntity），联结 User 聚合根和 Role 实体，
/// 构成用户-角色的多对多关系
/// </summary>
public class UserRole : AssociationEntity
{
    /// <summary>
    /// 用户标识，关联到 User 实体
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// 角色标识，关联到 Role 实体
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private UserRole() { }

    /// <summary>
    /// 创建用户-角色关联
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <param name="roleId">角色标识</param>
    public UserRole(Guid userId, Guid roleId) { UserId = userId; RoleId = roleId; }

    /// <summary>
    /// 导航属性：关联的角色实体（由 EF Core 延迟加载或显式加载填充）
    /// </summary>
    public Role? Role { get; private set; }
}
