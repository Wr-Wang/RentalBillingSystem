namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

/// <summary>
/// 角色仓储接口。
/// 定义角色聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按角色代码查询、按用户查询角色列表以及加载角色-菜单权限
/// 关联信息等业务查询方法。
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// 根据角色代码获取角色信息。
    /// 角色代码在系统范围内唯一。
    /// </summary>
    /// <param name="code">角色代码（如"Admin"、"Finance"等）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>角色实体，未找到时返回 null</returns>
    Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 根据用户 ID 获取该用户关联的所有角色。
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>角色列表</returns>
    Task<List<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// 根据角色 ID 获取角色及其关联的菜单权限信息。
    /// </summary>
    /// <param name="id">角色 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>包含菜单权限的角色实体，未找到时返回 null</returns>
    Task<Role?> GetByIdWithRoleMenusAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 保存角色的菜单权限（全量覆盖：先删后插）。
    /// 变更追踪无法感知 _roleMenus 集合变化，需直接操作 RoleMenus 表。
    /// </summary>
    /// <param name="roleId">角色 ID</param>
    /// <param name="menuIds">要分配的菜单 ID 列表</param>
    /// <param name="changedBy">操作人 ID</param>
    /// <param name="ct">取消令牌</param>
    Task SaveRoleMenusAsync(Guid roleId, List<Guid> menuIds, Guid changedBy, CancellationToken ct = default);
}
