using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class FeeCodeRepository : BaseRepository<FeeCode>, IFeeCodeRepository
{
    public FeeCodeRepository(AppDbContext context) : base(context) { }

    public async Task<FeeCode?> GetByCodeAsync(string code, Guid companyId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.Code == code && f.CompanyId == companyId, ct);
    }

    public async Task<List<FeeCode>> GetByCategoryAsync(string category, Guid companyId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(f => f.Category == category && f.CompanyId == companyId && f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ToListAsync(ct);
    }
}
