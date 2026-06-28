using Microsoft.AspNetCore.Http;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 多租户（多公司）服务实现
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? HomeCompanyId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("HomeCompanyId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("IsSuperAdmin");
            return claim != null && bool.TryParse(claim.Value, out var val) && val;
        }
    }

    public List<Guid> CompanyScope { get; } = new();

    /// <summary>
    /// 当前生效的 CompanyId（用于 Query Filter）
    /// 超管选择"全部数据"时返回 null，不过滤
    /// </summary>
    public Guid? EffectiveCompanyId
    {
        get
        {
            if (IsSuperAdmin)
            {
                var currentId = _httpContextAccessor.HttpContext?.Request
                    .Query["companyId"].FirstOrDefault();
                if (string.IsNullOrEmpty(currentId))
                    return null;
                return Guid.Parse(currentId);
            }

            return HomeCompanyId;
        }
    }

    public bool IsViewingAll => IsSuperAdmin && EffectiveCompanyId == null;
}
