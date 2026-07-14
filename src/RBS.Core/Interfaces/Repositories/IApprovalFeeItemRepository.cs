namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Approval;

/// <summary>
/// 审批调价明细仓储接口。
/// 定义审批调价明细聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 审批调价明细用于存储费用调整审批单中各费用项目的详细调整信息
/// （如原金额、新金额、生效日期等）。
/// </summary>
public interface IApprovalFeeItemRepository : IRepository<ApprovalFeeItem>
{
    /// <summary>
    /// 根据审批请求 ID 获取该审批单下所有的调价明细项。
    /// </summary>
    /// <param name="approvalRequestId">审批请求 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>调价明细列表</returns>
    Task<List<ApprovalFeeItem>> GetByApprovalRequestIdAsync(Guid approvalRequestId, CancellationToken ct = default);
}
