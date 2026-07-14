namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

/// <summary>
/// 支付渠道仓储接口。
/// 定义支付渠道聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按公司查询活跃支付渠道的业务方法。
/// </summary>
public interface IPaymentChannelRepository : IRepository<PaymentChannel>
{
    /// <summary>
    /// 获取指定公司下所有活跃的支付渠道。
    /// 用于收款时展示可用的支付方式（如银行转账、支付宝、微信等）。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>活跃支付渠道列表</returns>
    Task<List<PaymentChannel>> GetActiveByCompanyAsync(Guid companyId, CancellationToken ct = default);
}
