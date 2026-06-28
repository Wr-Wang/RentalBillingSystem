using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class MeterReadingRepository : BaseRepository<MeterReading>, IMeterReadingRepository
{
    public MeterReadingRepository(AppDbContext context) : base(context) { }

    public async Task<MeterReading?> GetLatestReadingAsync(Guid contractFeeConfigId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(m => m.ContractFeeConfigId == contractFeeConfigId && m.Status == "Confirmed")
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<MeterReading>> GetHistoryAsync(
        Guid contractFeeConfigId, int year, int month, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(m => m.ContractFeeConfigId == contractFeeConfigId)
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .Take(12)
            .ToListAsync(ct);
    }

    public async Task<bool> ReadingExistsAsync(
        Guid contractFeeConfigId, int year, int month, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(m => m.ContractFeeConfigId == contractFeeConfigId && m.Year == year && m.Month == month, ct);
    }
}
