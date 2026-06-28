using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class ReceivablePlanRepository : BaseRepository<ReceivablePlan>, IReceivablePlanRepository
{
    public ReceivablePlanRepository(AppDbContext context) : base(context) { }

    public async Task<List<ReceivablePlan>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(rp => rp.ContractId == contractId)
            .OrderBy(rp => rp.Period)
            .ToListAsync(ct);
    }

    public async Task<ReceivablePlan?> GetByContractPeriodFeeAsync(
        Guid contractId, string period, Guid feeCodeId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(rp => rp.ContractId == contractId && rp.Period == period && rp.FeeCodeId == feeCodeId, ct);
    }

    public async Task<List<ReceivablePlan>> GetOverdueAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(rp => rp.Status == "Pending" && rp.DueDate < DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync(ct);
    }
}
