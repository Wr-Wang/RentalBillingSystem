using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context) { }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Code == code, ct);
    }

    public async Task<List<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(r => r.UserRoles.Any(ur => ur.UserId == userId))
            .ToListAsync(ct);
    }

    public async Task<Role?> GetByIdWithRoleMenusAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(r => r.RoleMenus)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }
}
