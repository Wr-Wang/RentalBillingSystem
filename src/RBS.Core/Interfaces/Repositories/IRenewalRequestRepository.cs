using RBS.Core.Entities.Contract;

namespace RBS.Core.Interfaces.Repositories;

/// <summary>
/// 续签请求仓储接口。
/// 定义续签请求聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按旧合同查询、按合同查询待处理请求以及检查是否有待处理续签请求等业务方法。
/// </summary>
public interface IRenewalRequestRepository : IRepository<RenewalRequest>
{
    /// <summary>
    /// 根据旧合同 ID 获取所有续签请求。
    /// </summary>
    /// <param name="oldContractId">旧合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>续签请求列表</returns>
    Task<List<RenewalRequest>> GetByOldContractIdAsync(Guid oldContractId, CancellationToken ct = default);

    /// <summary>
    /// 获取指定合同当前待处理的续签请求。
    /// 每个合同同一时间只能有一个待处理的续签请求。
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>待处理续签请求，不存在时返回 null</returns>
    Task<RenewalRequest?> GetPendingByContractIdAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>
    /// 检查指定合同是否已有待处理的续签请求。
    /// 用于防止创建重复的续签申请。
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>存在待处理请求时返回 true，否则 false</returns>
    Task<bool> HasPendingForContractAsync(Guid contractId, CancellationToken ct = default);
}
