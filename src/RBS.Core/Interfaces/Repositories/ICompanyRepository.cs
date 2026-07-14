namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

/// <summary>
/// 公司仓储接口。
/// 定义公司聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按名称查询和获取活跃公司列表等业务查询方法。
/// </summary>
public interface ICompanyRepository : IRepository<Company>
{
    /// <summary>
    /// 根据公司名称获取公司信息。
    /// </summary>
    /// <param name="name">公司名称</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>公司实体，未找到时返回 null</returns>
    Task<Company?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// 获取所有活跃状态的公司列表。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>活跃公司列表</returns>
    Task<List<Company>> GetActiveAsync(CancellationToken ct = default);
}
