namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Contract;

/// <summary>
/// 租客仓储接口。
/// 定义租客聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按手机号查询和按关键字搜索等业务查询方法。
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    /// <summary>
    /// 根据手机号获取租客信息。
    /// </summary>
    /// <param name="phone">手机号码</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>租客实体，未找到时返回 null</returns>
    Task<Tenant?> GetByPhoneAsync(string phone, CancellationToken ct = default);

    /// <summary>
    /// 根据关键字搜索租客（按姓名、手机号等字段模糊匹配）。
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>匹配的租客列表</returns>
    Task<List<Tenant>> SearchAsync(string keyword, CancellationToken ct = default);
}
