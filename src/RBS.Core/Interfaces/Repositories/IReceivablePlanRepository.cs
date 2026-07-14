namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

/// <summary>
/// 应收计划仓储接口。
/// 定义应收计划聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按合同查询、按合同账期费用项目精确查询、以及获取逾期应收列表等业务查询。
/// </summary>
public interface IReceivablePlanRepository : IRepository<ReceivablePlan>
{
    /// <summary>
    /// 根据合同 ID 获取该合同下的所有应收计划。
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>应收计划列表</returns>
    Task<List<ReceivablePlan>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>
    /// 根据合同 ID、账期和费用项目 ID 精确获取应收计划。
    /// 用于避免同一合同同一账期生成重复的应收计划。
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="period">账期，格式为"yyyy-MM"</param>
    /// <param name="feeCodeId">费用项目 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>应收计划实体，未找到时返回 null</returns>
    Task<ReceivablePlan?> GetByContractPeriodFeeAsync(Guid contractId, string period, Guid feeCodeId, CancellationToken ct = default);

    /// <summary>
    /// 获取指定公司下所有逾期的应收计划。
    /// 用于滞纳金计算和催缴处理。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>逾期应收计划列表</returns>
    Task<List<ReceivablePlan>> GetOverdueAsync(Guid companyId, CancellationToken ct = default);
}
