using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class FeeCodeRepository : BaseRepository<FeeCode>, IFeeCodeRepository
{
    public FeeCodeRepository(AppDbContext context) : base(context) { }

    public async Task<FeeCode?> GetByCodeAsync(string code, Guid landlordId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.Code == code && f.LandlordId == landlordId, ct);
    }

    public async Task<List<FeeCode>> GetByCategoryAsync(string category, Guid landlordId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(f => f.Category == category && f.LandlordId == landlordId && f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ToListAsync(ct);
    }
}
