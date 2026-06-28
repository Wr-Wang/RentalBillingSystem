namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Role?> GetByIdWithRoleMenusAsync(Guid id, CancellationToken ct = default);
}
