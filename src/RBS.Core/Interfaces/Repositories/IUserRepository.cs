namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

/// <summary>
/// 用户仓储接口。
/// 定义用户聚合根的特有查询和操作方法，继承泛型 CRUD 操作。
/// 提供按用户名查询、角色加载、权限查询、用户名唯一性校验以及角色替换等业务方法。
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// 根据用户名获取用户。
    /// </summary>
    /// <param name="username">用户名（唯一）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户实体，未找到时返回 null</returns>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// 根据用户 ID 获取用户及其关联的角色信息。
    /// </summary>
    /// <param name="id">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户实体（包含角色导航属性），未找到时返回 null</returns>
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 获取所有用户及其关联的角色信息。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户列表（每个用户包含角色导航属性）</returns>
    Task<List<User>> GetAllWithRolesAsync(CancellationToken ct = default);

    /// <summary>
    /// 获取指定用户拥有的所有权限代码列表。
    /// 权限从用户关联的角色-菜单映射中解析得出。
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>权限代码字符串列表</returns>
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// 检查用户名是否唯一（可选地排除指定用户 ID）。
    /// </summary>
    /// <param name="username">待检查的用户名</param>
    /// <param name="excludeId">排除的用户 ID（修改时排除自身），可为 null</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>唯一时返回 true，否则 false</returns>
    Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// 替换用户角色（先删后增，原始 SQL 实现，绕过 EF Core 跟踪）。
    /// 用于批量更新用户关联的角色集合，事务性地删除旧角色并插入新角色。
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="newRoleIds">新角色 ID 列表</param>
    /// <param name="changedBy">操作人用户 ID</param>
    /// <param name="ct">取消令牌</param>
    Task ReplaceRolesAsync(Guid userId, List<Guid> newRoleIds, Guid changedBy, CancellationToken ct = default);
}
