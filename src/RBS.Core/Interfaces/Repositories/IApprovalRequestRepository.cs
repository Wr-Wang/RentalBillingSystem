namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Approval;

public interface IApprovalRequestRepository : IRepository<ApprovalRequest>
{
    Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default);
    Task<List<ApprovalRequest>> GetByTargetAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default);
}
