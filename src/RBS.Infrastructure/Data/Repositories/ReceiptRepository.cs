using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class ReceiptRepository : BaseRepository<Receipt>, IReceiptRepository
{
    public ReceiptRepository(AppDbContext context) : base(context) { }

    public async Task<List<Receipt>> GetPendingConfirmAsync(Guid landlordId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(r => r.LandlordId == landlordId && r.Status == "Pending")
            .OrderByDescending(r => r.ReceivedDate)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalConfirmedAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(r => r.ContractId == contractId && r.Status == "Confirmed")
            .SumAsync(r => (decimal?)r.Amount) ?? 0;
    }
}
