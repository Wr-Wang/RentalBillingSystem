namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 当前用户信息服务 — 从 HttpContext 解析当前登录用户。
/// 提供对当前请求上下文中认证用户信息的统一访问接口，
/// 避免应用层和领域层直接依赖 ASP.NET Core 的 HttpContext。
/// 实现层在 Web 项目中通过注入 IHttpContextAccessor 解析 JWT Token
/// 或 Cookie 中保存的用户信息来填充此接口。
/// </summary>
public interface ICurrentUserService
{
    /// <summary>当前用户的唯一标识 ID</summary>
    Guid UserId { get; }

    /// <summary>当前用户的登录名，未认证时返回 null</summary>
    string? Username { get; }

    /// <summary>当前用户是否为超级管理员（不受公司数据范围限制）</summary>
    bool IsSuperAdmin { get; }

    /// <summary>当前用户所属公司的 ID，超管可能为 null</summary>
    Guid? CompanyId { get; }

    /// <summary>当前用户拥有的角色 ID 列表</summary>
    List<Guid> RoleIds { get; }

    /// <summary>当前用户拥有的权限代码列表（从角色-菜单映射解析得出）</summary>
    List<string> Permissions { get; }
}
