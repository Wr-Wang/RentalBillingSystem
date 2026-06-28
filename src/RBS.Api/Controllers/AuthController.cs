using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserEntity = RBS.Core.Entities.Organization.User;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _uow;

    public AuthController(IConfiguration configuration, IUnitOfWork uow)
    {
        _configuration = configuration;
        _uow = uow;
    }

    public record LoginRequest(string Username, string Password);

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

        // TODO: 使用 BCrypt 验证密码 Hash
        if (user.PasswordHash != request.Password)
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
                user.HomeLandlordId,
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
            user.HomeLandlordId,
            user.IsSuperAdmin,
            Roles = roles.Select(r => new { r.Id, r.Name, r.Code }),
            Permissions = permissions
        });
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
            new("HomeLandlordId", user.HomeLandlordId?.ToString() ?? "")
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
