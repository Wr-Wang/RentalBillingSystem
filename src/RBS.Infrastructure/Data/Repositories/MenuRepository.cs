using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class MenuRepository : BaseRepository<Menu>, IMenuRepository
{
    public MenuRepository(AppDbContext context) : base(context) { }

    public async Task<List<Menu>> GetByRoleIdsAsync(List<Guid> roleIds, CancellationToken ct = default)
    {
        return await _context.Set<RoleMenu>()
            .Where(rm => roleIds.Contains(rm.RoleId))
            .Select(rm => rm.Menu!)
            .Where(m => m.IsActive)
            .Distinct()
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<List<Menu>> GetTreeAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);
    }
}
