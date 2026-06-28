using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class PaymentChannelRepository : BaseRepository<PaymentChannel>, IPaymentChannelRepository
{
    public PaymentChannelRepository(AppDbContext context) : base(context) { }

    public async Task<List<PaymentChannel>> GetActiveByLandlordAsync(Guid landlordId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p => p.LandlordId == landlordId && p.IsActive)
            .ToListAsync(ct);
    }
}
