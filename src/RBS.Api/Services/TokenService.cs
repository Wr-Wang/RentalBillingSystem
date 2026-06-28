using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RBS.Application.Services.Organization;
using RBS.Core.Entities.Organization;

namespace RBS.Api.Services;

/// <summary>
/// JWT Token 生成服务
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
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
