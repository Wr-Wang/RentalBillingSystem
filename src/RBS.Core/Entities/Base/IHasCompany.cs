namespace RBS.Core.Entities.Base;

/// <summary>
/// 多租户（多公司）数据隔离接口
/// 实现此接口的实体将自动附加 CompanyId 查询过滤器
/// </summary>
public interface IHasCompany
{
    Guid CompanyId { get; }
}
