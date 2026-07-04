namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Approval;

/// <summary>
/// 审批调价明细仓储接口
/// </summary>
public interface IApprovalFeeItemRepository : IRepository<ApprovalFeeItem>
{
    Task<List<ApprovalFeeItem>> GetByApprovalRequestIdAsync(Guid approvalRequestId, CancellationToken ct = default);
}
