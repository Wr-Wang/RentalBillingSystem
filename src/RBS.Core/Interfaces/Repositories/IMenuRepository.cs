namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

/// <summary>
/// 菜单仓储接口。
/// 定义菜单聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按角色查询菜单列表以及获取菜单树等业务查询方法。
/// 用于权限控制和导航菜单构建。
/// </summary>
public interface IMenuRepository : IRepository<Menu>
{
    /// <summary>
    /// 根据角色 ID 列表获取对应的菜单权限列表。
    /// 返回所有被授权角色所拥有的菜单集合，用于构建用户的导航菜单和权限验证。
    /// </summary>
    /// <param name="roleIds">角色 ID 列表</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>菜单列表</returns>
    Task<List<Menu>> GetByRoleIdsAsync(List<Guid> roleIds, CancellationToken ct = default);

    /// <summary>
    /// 获取菜单树形结构（按父子关系组织）。
    /// 用于前端导航菜单的树形展示和后端权限配置。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>菜单列表（含父子关系）</returns>
    Task<List<Menu>> GetTreeAsync(CancellationToken ct = default);
}
