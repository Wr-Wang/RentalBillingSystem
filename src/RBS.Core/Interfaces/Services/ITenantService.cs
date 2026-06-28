namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 多租户（多房东）服务 — 管理当前用户的房东数据隔离范围
/// </summary>
public interface ITenantService
{
    /// <summary>当前用户的所属房东ID</summary>
    Guid? HomeLandlordId { get; }

    /// <summary>当前用户是否为超级管理员</summary>
    bool IsSuperAdmin { get; }

    /// <summary>当前用户可查看的房东ID列表</summary>
    List<Guid> LandlordScope { get; }

    /// <summary>视角切换——当前生效的LandlordId</summary>
    Guid? EffectiveLandlordId { get; }

    /// <summary>是否正在查看全部数据（超管专用）</summary>
    bool IsViewingAll { get; }
}
