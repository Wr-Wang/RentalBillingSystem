namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Approval;

public interface IApprovalRequestRepository : IRepository<ApprovalRequest>
{
    Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default);
    Task<List<ApprovalRequest>> GetByTargetAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default);
    Task<List<ApprovalRequest>> GetByApproverAsync(Guid userId, CancellationToken ct = default);
    Task<ApprovalRequest?> GetByIdWithRecordsAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ApprovalRequest>> GetHistoryAsync(Guid userId, string? keyword, string? status, int page, int pageSize, CancellationToken ct = default);
}
