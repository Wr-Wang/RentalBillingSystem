namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

/// <summary>
/// 收款单仓储接口。
/// 定义收款单聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供待确认收款单查询、按公司查询、以及合同已确认收款总额查询等业务方法。
/// </summary>
public interface IReceiptRepository : IRepository<Receipt>
{
    /// <summary>
    /// 获取指定公司下所有待确认的收款单。
    /// 用于财务人员进行收款确认操作。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>待确认收款单列表</returns>
    Task<List<Receipt>> GetPendingConfirmAsync(Guid companyId, CancellationToken ct = default);

    /// <summary>
    /// 获取指定公司下的所有收款单。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>收款单列表</returns>
    Task<List<Receipt>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default);

    /// <summary>
    /// 获取指定合同下已确认收款的总金额。
    /// 用于应收计划的已收金额汇总和对账。
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>已确认收款总额</returns>
    Task<decimal> GetTotalConfirmedAsync(Guid contractId, CancellationToken ct = default);
}
