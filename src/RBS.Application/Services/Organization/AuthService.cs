using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

/// <summary>
/// 认证应用服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;

    public AuthService(IUnitOfWork uow, ITokenService tokenService)
    {
        _uow = uow;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByUsernameAsync(username, ct);
        if (user == null || user.PasswordHash != password || !user.IsActive)
            throw new UnauthorizedAccessException("用户名或密码错误");

        var permissions = await _uow.Users.GetUserPermissionsAsync(user.Id, ct);
        var roles = await _uow.Roles.GetByUserIdAsync(user.Id, ct);

        return new LoginResponse
        {
            Token = _tokenService.GenerateToken(user),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Phone = user.Phone,
                Email = user.Email,
                HomeCompanyId = user.HomeCompanyId,
                IsSuperAdmin = user.IsSuperAdmin
            },
            Roles = roles.Select(r => new RoleInfo { Id = r.Id, Name = r.Name, Code = r.Code }).ToList(),
            Permissions = permissions
        };
    }

    public async Task<UserInfo> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null) throw new KeyNotFoundException("用户不存在");

        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Phone = user.Phone,
            Email = user.Email,
            HomeCompanyId = user.HomeCompanyId,
            IsSuperAdmin = user.IsSuperAdmin
        };
    }
}

/// <summary>
/// Token 生成服务接口
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
}
