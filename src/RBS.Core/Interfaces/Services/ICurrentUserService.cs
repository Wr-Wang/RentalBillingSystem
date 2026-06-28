namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 当前用户信息服务 — 从 HttpContext 解析当前登录用户
/// </summary>
public interface ICurrentUserService
{
    /// <summary>当前用户 ID</summary>
    Guid UserId { get; }

    /// <summary>当前用户名</summary>
    string? Username { get; }

    /// <summary>是否为超级管理员</summary>
    bool IsSuperAdmin { get; }

    /// <summary>用户所属房东 ID</summary>
    Guid? HomeLandlordId { get; }

    /// <summary>用户拥有的角色 ID 列表</summary>
    List<Guid> RoleIds { get; }

    /// <summary>用户的权限代码列表</summary>
    List<string> Permissions { get; }
}
