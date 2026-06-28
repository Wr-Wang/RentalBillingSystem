using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class TenantRepository : BaseRepository<Tenant>, ITenantRepository
{
    public TenantRepository(AppDbContext context) : base(context) { }

    public async Task<Tenant?> GetByPhoneAsync(string phone, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Phone == phone, ct);
    }

    public async Task<List<Tenant>> SearchAsync(string keyword, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(t => t.Name.Contains(keyword) || (t.Phone != null && t.Phone.Contains(keyword)))
            .Take(20)
            .ToListAsync(ct);
    }
}
