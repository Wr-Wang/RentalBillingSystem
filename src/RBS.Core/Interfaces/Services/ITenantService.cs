namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 多租户（多公司）服务 — 管理当前用户的公司数据隔离范围
/// </summary>
public interface ITenantService
{
    /// <summary>当前用户的所属公司ID</summary>
    Guid? HomeCompanyId { get; }

    /// <summary>当前用户是否为超级管理员</summary>
    bool IsSuperAdmin { get; }

    /// <summary>当前用户可查看的公司ID列表</summary>
    List<Guid> CompanyScope { get; }

    /// <summary>视角切换——当前生效的CompanyId</summary>
    Guid? EffectiveCompanyId { get; }

    /// <summary>是否正在查看全部数据（超管专用）</summary>
    bool IsViewingAll { get; }
}
