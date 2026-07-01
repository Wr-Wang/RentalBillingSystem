namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
    Task<List<User>> GetAllWithRolesAsync(CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken ct = default);
    /// <summary>替换用户角色（先删后增，原始 SQL 实现，绕过 EF Core 跟踪）</summary>
    Task ReplaceRolesAsync(Guid userId, List<Guid> newRoleIds, Guid changedBy, CancellationToken ct = default);
}
