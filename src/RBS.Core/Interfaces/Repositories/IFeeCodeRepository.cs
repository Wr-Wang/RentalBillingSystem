namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

/// <summary>
/// 费用项目仓储接口。
/// 定义费用项目聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按代码查询和按分类查询等业务查询方法。
/// </summary>
public interface IFeeCodeRepository : IRepository<FeeCode>
{
    /// <summary>
    /// 根据费用代码和公司获取费用项目。
    /// 费用代码在公司范围内唯一。
    /// </summary>
    /// <param name="code">费用代码（如"Rent"、"ManagementFee"等）</param>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>费用项目实体，未找到时返回 null</returns>
    Task<FeeCode?> GetByCodeAsync(string code, Guid companyId, CancellationToken ct = default);

    /// <summary>
    /// 根据分类和公司获取费用项目列表。
    /// </summary>
    /// <param name="category">费用分类</param>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>费用项目列表</returns>
    Task<List<FeeCode>> GetByCategoryAsync(string category, Guid companyId, CancellationToken ct = default);
}
