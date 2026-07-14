using RBS.Application.DTOs.Organization;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 认证应用服务接口 — 提供用户登录认证与当前用户信息查询能力
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码（明文，内部使用 BCrypt 验证）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>登录响应，包含 JWT Token、用户信息、角色列表、权限集合及公司列表</returns>
    /// <exception cref="UnauthorizedAccessException">用户名或密码错误时抛出</exception>
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct = default);

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户基本信息（不含 Token/角色/权限）</returns>
    /// <exception cref="KeyNotFoundException">用户不存在时抛出</exception>
    Task<UserInfo> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
