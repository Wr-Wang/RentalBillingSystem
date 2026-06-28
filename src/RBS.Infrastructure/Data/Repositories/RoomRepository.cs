using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.Repositories;
using ContractEntity = RBS.Core.Entities.Contract.Contract;

namespace RBS.Infrastructure.Data.Repositories;

public class RoomRepository : BaseRepository<Room>, IRoomRepository
{
    public RoomRepository(AppDbContext context) : base(context) { }

    public async Task<List<Room>> GetByBuildingIdAsync(Guid buildingId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(r => r.BuildingId == buildingId)
            .OrderBy(r => r.FullCode)
            .ToListAsync(ct);
    }

    public async Task<Room?> GetByFullCodeAsync(string fullCode, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.FullCode == fullCode, ct);
    }

    public async Task<bool> HasActiveContractAsync(Guid roomId, CancellationToken ct = default)
    {
        return await _context.Set<ContractEntity>()
            .AnyAsync(c => c.RoomId == roomId && c.StatusCode == "Active", ct);
    }
}
