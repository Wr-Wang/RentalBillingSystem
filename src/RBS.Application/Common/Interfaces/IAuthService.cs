using RBS.Application.DTOs.Organization;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 认证应用服务
/// </summary>
public interface IAuthService
{
    /// <summary>用户登录</summary>
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct = default);

    /// <summary>获取当前用户信息</summary>
    Task<UserInfo> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
