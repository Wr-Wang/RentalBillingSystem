using Microsoft.AspNetCore.Http;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 多租户（多公司）服务实现 — 从 HttpContext 中的 JWT 声明和查询参数解析当前公司信息
/// </summary>
/// <remarks>
/// 核心逻辑：
/// <list type="bullet">
///   <item><description>CompanyId 从 JWT Claim "CompanyId" 中读取</description></item>
///   <item><description>IsSuperAdmin 从 JWT Claim "IsSuperAdmin" 中读取</description></item>
///   <item><description>EffectiveCompanyId：超管可通过查询参数 ?companyId=xxx 切换公司；普通用户直接使用 CompanyId</description></item>
///   <item><description>DefaultCompanyId：写入操作使用的公司 ID，优先级：EffectiveCompanyId → DefaultCompanyId(Claim) → CompanyId</description></item>
///   <item><description>IsViewingAll：超管且未指定公司 ID 时为"查看全部数据"模式</description></item>
/// </list>
/// 设计模式：基于 HttpContext 的请求范围多租户解析。
/// </remarks>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 初始化多租户服务
    /// </summary>
    /// <param name="httpContextAccessor">HttpContext 访问器，用于读取 JWT Claims</param>
    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CompanyId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("CompanyId");
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

            return CompanyId;
        }
    }

    public bool IsViewingAll => IsSuperAdmin && EffectiveCompanyId == null;

    private Guid? DefaultCompanyIdFromClaim
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("DefaultCompanyId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    /// <summary>
    /// 默认公司（用于写入操作）
    /// 优先级：EffectiveCompanyId → DefaultCompanyId(DB持久化) → CompanyId
    /// </summary>
    public Guid DefaultCompanyId => EffectiveCompanyId ?? DefaultCompanyIdFromClaim ?? CompanyId ?? Guid.Empty;
}
