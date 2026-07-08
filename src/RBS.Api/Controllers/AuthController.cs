using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RBS.Core.Interfaces.Services;
using UserEntity = RBS.Core.Entities.Organization.User;
using RBS.Core.Interfaces.UnitOfWork;
using BCryptNet = BCrypt.Net.BCrypt;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IConfiguration configuration, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _configuration = configuration;
        _uow = uow;
        _currentUser = currentUser;
    }

    public record LoginRequest(string Username, string Password);

    public record ChangePasswordRequest(string OldPassword, string NewPassword);

    /// <summary>
    /// 用户登录 — 验证凭据并返回 JWT Token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByUsernameAsync(request.Username, ct);
        if (user == null)
            return Unauthorized(new { Message = "用户名或密码错误" });

        // BCrypt 验证密码
        if (!BCryptNet.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { Message = "用户名或密码错误" });

        if (!user.IsActive)
            return Unauthorized(new { Message = "账户已被禁用" });

        var token = GenerateJwtToken(user);
        var permissions = await _uow.Users.GetUserPermissionsAsync(user.Id, ct);
        var roles = await _uow.Roles.GetByUserIdAsync(user.Id, ct);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.Username,
                user.DisplayName,
                user.Phone,
                user.Email,
                user.CompanyId,
                DefaultCompanyId = user.DefaultCompanyId ?? user.CompanyId,
                user.IsSuperAdmin
            },
            Roles = roles.Select(r => new { r.Id, r.Name, r.Code }),
            Permissions = permissions
        });
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null)
            return NotFound();

        var permissions = await _uow.Users.GetUserPermissionsAsync(userId, ct);
        var roles = await _uow.Roles.GetByUserIdAsync(userId, ct);

        return Ok(new
        {
            user.Id,
            user.Username,
            user.DisplayName,
            user.Phone,
            user.Email,
            user.CompanyId,
            DefaultCompanyId = user.DefaultCompanyId ?? user.CompanyId,
            user.IsSuperAdmin,
            Roles = roles.Select(r => new { r.Id, r.Name, r.Code }),
            Permissions = permissions
        });
    }

    /// <summary>
    /// 修改当前用户密码
    /// </summary>
    [HttpPost("changepassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "参数不能为空" });

        if (request.OldPassword == request.NewPassword)
            return BadRequest(new { message = "新密码不能与旧密码相同" });

        var userId = _currentUser.UserId;
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null)
            return NotFound(new { message = "用户不存在" });

        // BCrypt 验证原密码
        if (!BCryptNet.Verify(request.OldPassword, user.PasswordHash))
            return BadRequest(new { message = "原密码不正确" });

        user.ChangePassword(BCryptNet.HashPassword(request.NewPassword));
        await _uow.Users.UpdateAsync(user, ct);
        await _uow.CommitAsync(ct);

        return Ok(new { message = "密码修改成功" });
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJwtTokenGenerationAtLeast32Chars!";
        var issuer = jwtSettings["Issuer"] ?? "RBS";
        var audience = jwtSettings["Audience"] ?? "RBS";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "120");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("DisplayName", user.DisplayName),
            new("IsSuperAdmin", user.IsSuperAdmin.ToString()),
            new("CompanyId", user.CompanyId?.ToString() ?? ""),
            new("DefaultCompanyId", (user.DefaultCompanyId ?? user.CompanyId)?.ToString() ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
