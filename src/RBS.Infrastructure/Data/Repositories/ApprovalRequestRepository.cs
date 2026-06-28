using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Approval;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class ApprovalRequestRepository : BaseRepository<ApprovalRequest>, IApprovalRequestRepository
{
    public ApprovalRequestRepository(AppDbContext context) : base(context) { }

    public async Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(a => a.Status == "Pending")
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ApprovalRequest>> GetByTargetAsync(
        Guid targetEntityId, string targetEntityType, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(a => a.TargetEntityId == targetEntityId && a.TargetEntityType == targetEntityType)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}
