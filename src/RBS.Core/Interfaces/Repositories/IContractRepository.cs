namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Contract;

/// <summary>
/// 合同仓储接口。
/// 定义合同聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按合同号查询、获取生效合同列表、到期合同提醒以及
/// 检查房屋单元是否已有生效合同等业务查询。
/// </summary>
public interface IContractRepository : IRepository<Contract>
{
    /// <summary>
    /// 根据合同编号获取合同。
    /// </summary>
    /// <param name="contractNo">合同编号（唯一）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>合同实体，未找到时返回 null</returns>
    Task<Contract?> GetByContractNoAsync(string contractNo, CancellationToken ct = default);

    /// <summary>
    /// 获取指定公司下所有生效中的合同列表。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>生效合同列表</returns>
    Task<List<Contract>> GetActiveContractsAsync(Guid companyId, CancellationToken ct = default);

    /// <summary>
    /// 获取在指定日期到期的合同列表，用于到期提醒和续签处理。
    /// </summary>
    /// <param name="date">到期日期</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>到期合同列表</returns>
    Task<List<Contract>> GetContractsExpiringAsync(DateTime date, CancellationToken ct = default);

    /// <summary>
    /// 检查指定房屋单元是否已有生效合同。
    /// 用于防止同一房间重复签约。
    /// </summary>
    /// <param name="housingUnitId">房屋单元 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>存在生效合同时返回 true，否则 false</returns>
    Task<bool> HasActiveForHousingUnitAsync(Guid housingUnitId, CancellationToken ct = default);
}
