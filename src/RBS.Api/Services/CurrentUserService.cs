using System.Security.Claims;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Services;

/// <summary>
/// 当前用户信息服务 — 从 HttpContext 解析 JWT Claims
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null && Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User
        ?.FindFirst(ClaimTypes.Name)?.Value;

    public bool IsSuperAdmin
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("IsSuperAdmin")?.Value;
            return bool.TryParse(claim, out var val) && val;
        }
    }

    public Guid? HomeLandlordId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("HomeLandlordId")?.Value;
            return claim != null && Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public List<Guid> RoleIds
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User
                ?.FindAll(ClaimTypes.Role);
            return claims?.Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList() ?? new();
        }
    }

    public List<string> Permissions
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User
                ?.FindAll("Permission");
            return claims?.Select(c => c.Value).ToList() ?? new();
        }
    }
}
