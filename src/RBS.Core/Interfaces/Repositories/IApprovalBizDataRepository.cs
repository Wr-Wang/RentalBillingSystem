namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Approval;

/// <summary>
/// 审批业务数据仓储接口
/// </summary>
public interface IApprovalBizDataRepository : IRepository<ApprovalBizData>
{
    Task<ApprovalBizData?> GetByApprovalRequestIdAsync(Guid approvalRequestId, CancellationToken ct = default);
    Task<List<ApprovalBizData>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
}
