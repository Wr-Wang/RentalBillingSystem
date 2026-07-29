namespace RBS.Application.DTOs.SystemConfig;

/// <summary>
/// 系统日志条目 DTO
/// </summary>
public class SystemLogDto
{
    public Guid Id { get; set; }
    public string Level { get; set; } = null!;
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Guid? UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}
