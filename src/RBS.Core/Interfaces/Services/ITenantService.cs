namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 多租户（多公司）服务 — 管理当前用户的公司数据隔离范围。
/// 支持超管查看全部数据以及用户在公司间的视角切换。
/// 所有业务查询和写入操作应通过此服务确定数据访问范围，
/// 确保不同公司之间的数据安全隔离。
/// </summary>
public interface ITenantService
{
    /// <summary>当前用户的所属公司 ID，超管可能为空</summary>
    Guid? CompanyId { get; }

    /// <summary>当前用户是否为超级管理员（可查看和管理所有公司数据）</summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// 视角切换 —— 当前生效的 CompanyId。
    /// 普通用户等于 CompanyId，超管在切换公司视角后变化。
    /// </summary>
    Guid? EffectiveCompanyId { get; }

    /// <summary>
    /// 默认公司（用于写入操作）。
    /// 取值优先级：EffectiveCompanyId → CompanyScope[0] → CompanyId。
    /// 确保写入操作始终有确定的目标公司。
    /// </summary>
    Guid DefaultCompanyId { get; }

    /// <summary>是否正在查看全部数据（超管专用，用于跨公司查询场景）</summary>
    bool IsViewingAll { get; }
}
