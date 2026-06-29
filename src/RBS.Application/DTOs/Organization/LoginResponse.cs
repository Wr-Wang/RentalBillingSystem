namespace RBS.Application.DTOs.Organization;

/// <summary>
/// 登录响应 DTO
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
    public List<RoleInfo> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? HomeCompanyId { get; set; }
    public bool IsSuperAdmin { get; set; }
    public Guid? DefaultCompanyId { get; set; }
    public List<CompanyInfo> CompanyList { get; set; } = new();
}

public class CompanyInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RoleInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
