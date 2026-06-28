using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Repositories;
using RBS.Infrastructure.Data;

namespace RBS.Infrastructure.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Set<RoleMenu>()
            .Where(rm => rm.Role!.UserRoles.Any(ur => ur.UserId == userId))
            .Where(rm => rm.Menu!.PermissionCode != null)
            .Select(rm => rm.Menu!.PermissionCode!)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken ct = default)
    {
        if (excludeId.HasValue)
            return !await _dbSet.AnyAsync(u => u.Username == username && u.Id != excludeId.Value, ct);
        return !await _dbSet.AnyAsync(u => u.Username == username, ct);
    }
}
