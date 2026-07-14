namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Approval;

/// <summary>
/// 审批请求仓储接口。
/// 定义审批请求聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按审批人查询待审批项、按目标实体查询、获取审批历史记录
/// 以及加载审批记录详情等业务查询方法。
/// </summary>
public interface IApprovalRequestRepository : IRepository<ApprovalRequest>
{
    /// <summary>
    /// 获取指定用户待审批的请求列表。
    /// 根据用户的审批权限过滤当前处于对应级别的待审批请求。
    /// </summary>
    /// <param name="userId">审批人用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>待审批请求列表</returns>
    Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// 根据目标实体 ID 和类型获取关联的所有审批请求。
    /// 用于查看某个业务单据（如合同、续签等）的审批历程。
    /// </summary>
    /// <param name="targetEntityId">目标实体 ID</param>
    /// <param name="targetEntityType">目标实体类型（如"Contract"、"Renewal"等）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批请求列表</returns>
    Task<List<ApprovalRequest>> GetByTargetAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default);

    /// <summary>
    /// 获取指定用户作为审批人的所有审批请求（含已处理和待处理）。
    /// </summary>
    /// <param name="userId">审批人用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批请求列表</returns>
    Task<List<ApprovalRequest>> GetByApproverAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// 根据 ID 获取审批请求及其完整的审批记录（审批操作流水）。
    /// </summary>
    /// <param name="id">审批请求 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>包含审批记录的审批请求实体，未找到时返回 null</returns>
    Task<ApprovalRequest?> GetByIdWithRecordsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 分页查询指定用户的审批历史记录。
    /// 支持按关键字和状态条件过滤。
    /// </summary>
    /// <param name="userId">用户 ID（查询该用户相关的审批历史）</param>
    /// <param name="keyword">搜索关键字（按标题或描述模糊匹配），可为 null</param>
    /// <param name="status">审批状态过滤条件，可为 null</param>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>分页的审批请求结果</returns>
    Task<PagedResult<ApprovalRequest>> GetHistoryAsync(Guid userId, string? keyword, string? status, int page, int pageSize, CancellationToken ct = default);
}
