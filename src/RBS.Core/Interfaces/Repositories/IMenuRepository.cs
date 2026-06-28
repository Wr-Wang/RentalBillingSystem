namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

public interface IMenuRepository : IRepository<Menu>
{
    Task<List<Menu>> GetByRoleIdsAsync(List<Guid> roleIds, CancellationToken ct = default);
    Task<List<Menu>> GetTreeAsync(CancellationToken ct = default);
}
