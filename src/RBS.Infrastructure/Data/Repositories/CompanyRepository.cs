using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
{
    public CompanyRepository(AppDbContext context) : base(context) { }

    public async Task<Company?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.Name == name, ct);
    }

    public async Task<List<Company>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _dbSet.Where(l => l.IsActive).ToListAsync(ct);
    }
}
