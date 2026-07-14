namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Approval;

/// <summary>
/// 审批业务数据仓储接口。
/// 定义审批业务数据聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按审批请求 ID 和按合同 ID 查询审批业务数据的业务方法。
/// 审批业务数据存储审批过程中提交的业务变更信息（如调价明细等）。
/// </summary>
public interface IApprovalBizDataRepository : IRepository<ApprovalBizData>
{
    /// <summary>
    /// 根据审批请求 ID 获取关联的审批业务数据。
    /// </summary>
    /// <param name="approvalRequestId">审批请求 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批业务数据实体，不存在时返回 null</returns>
    Task<ApprovalBizData?> GetByApprovalRequestIdAsync(Guid approvalRequestId, CancellationToken ct = default);

    /// <summary>
    /// 根据合同 ID 获取该合同相关的所有审批业务数据。
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批业务数据列表</returns>
    Task<List<ApprovalBizData>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
}
