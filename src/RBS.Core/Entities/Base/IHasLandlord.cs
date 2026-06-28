namespace RBS.Core.Entities.Base;

/// <summary>
/// 多租户（多房东）数据隔离接口
/// 实现此接口的实体将自动附加 LandlordId 查询过滤器
/// </summary>
public interface IHasLandlord
{
    Guid LandlordId { get; }
}
