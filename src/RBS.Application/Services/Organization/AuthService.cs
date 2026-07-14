using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.UnitOfWork;
using BCryptNet = BCrypt.Net.BCrypt;

namespace RBS.Application.Services.Organization;

/// <summary>
/// 认证应用服务实现 — 使用 BCrypt 验证密码、JWT 生成 Token
/// 登录时加载用户基本信息、角色、权限及所有可选公司列表
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="uow">工作单元，提供用户仓储</param>
    /// <param name="tokenService">Token 生成服务</param>
    public AuthService(IUnitOfWork uow, ITokenService tokenService)
    {
        _uow = uow;
        _tokenService = tokenService;
    }

    /// <summary>
    /// 用户登录认证 — BCrypt 验证密码，成功后返回 Token 及完整用户信息
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码明文</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>登录响应（Token + 用户信息 + 角色 + 权限 + 公司列表）</returns>
    /// <exception cref="UnauthorizedAccessException">用户名或密码错误</exception>
    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByUsernameAsync(username, ct);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("用户名或密码错误");

        if (!BCryptNet.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("用户名或密码错误");

        var permissions = await _uow.Users.GetUserPermissionsAsync(user.Id, ct);
        var roles = await _uow.Roles.GetByUserIdAsync(user.Id, ct);

        // 加载所有公司列表（供前端下拉选择）
        var allCompanies = await _uow.Companies.GetAllAsync(ct);
        var companyList = allCompanies.Select(c => new CompanyInfo
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

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
                CompanyId = user.CompanyId,
                IsSuperAdmin = user.IsSuperAdmin,
                DefaultCompanyId = user.DefaultCompanyId,
                CompanyList = companyList
            },
            Roles = roles.Select(r => new RoleInfo { Id = r.Id, Name = r.Name, Code = r.Code }).ToList(),
            Permissions = permissions
        };
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户信息 DTO</returns>
    /// <exception cref="KeyNotFoundException">用户不存在</exception>
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
            CompanyId = user.CompanyId,
            IsSuperAdmin = user.IsSuperAdmin
        };
    }
}

/// <summary>
/// Token 生成服务接口 — 基于用户信息生成 JWT Token
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 生成 JWT Token
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>JWT Token 字符串</returns>
    string GenerateToken(User user);
}
