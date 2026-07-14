namespace RBS.Core.Entities.Base;

/// <summary>
/// 多公司（多租户）数据隔离标记接口
///
/// DDD 角色：数据层级隔离（Data-Level Tenant Isolation），
/// 将 CompanyId 作为租户标识附加到每个需要按公司隔离的数据实体上。
///
/// 实现效果：
/// - 实现此接口的实体在数据库查询时，由基础设施层（如仓储拦截器、EF Core Query Filter）
///   自动附加 "WHERE CompanyId = @currentCompanyId" 过滤条件，实现数据行级安全隔离
/// - 避免不同公司之间的数据越权访问
///
/// 使用规范：
/// - 所有多公司共享的业务实体（合同、收款、应收、房源等）都应实现此接口
/// - 系统级配置表（如系统参数、审批类型）不应实现此接口
/// - CompanyId 在实体创建时由领域工厂根据当前登录用户的所属公司赋值，之后不应变更
/// </summary>
public interface IHasCompany
{
    /// <summary>
    /// 所属公司 ID
    /// 标识本条数据属于哪个公司（租户）。用于多公司架构下的数据隔离。
    /// 该值在数据创建时确定，业务上不允许跨公司转移。
    /// </summary>
    Guid CompanyId { get; }
}
