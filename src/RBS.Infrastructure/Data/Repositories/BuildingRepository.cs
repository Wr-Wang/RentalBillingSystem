using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class BuildingRepository : BaseRepository<Building>, IBuildingRepository
{
    public BuildingRepository(AppDbContext context) : base(context) { }

    public async Task<List<Building>> GetByCompanyIdAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(b => b.Floors)
                .ThenInclude(f => f.Rooms)
            .Where(b => b.CompanyId == companyId)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
    }
}
