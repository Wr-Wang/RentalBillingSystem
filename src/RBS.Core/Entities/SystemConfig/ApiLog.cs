namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Common;

/// <summary>
/// API 调用日志 — 记录每次请求的完整上下文
/// </summary>
public class ApiLog
{
    public Guid Id { get; private set; }

    // ===== 用户信息 =====
    public Guid? UserId { get; private set; }
    public string? UserDisplayName { get; private set; }

    // ===== 请求信息 =====
    public string HttpMethod { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string? QueryString { get; private set; }
    public string? RequestBody { get; private set; }

    // ===== 响应信息 =====
    public int StatusCode { get; private set; }
    public string? ResponseBody { get; private set; }
    public long DurationMs { get; private set; }

    // ===== 网络信息 =====
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    // ===== 时间 =====
    public DateTime CreatedAt { get; private set; }

    private ApiLog() { }

    public ApiLog(
        string httpMethod, string path, string? queryString, string? requestBody,
        int statusCode, string? responseBody, long durationMs,
        string? ipAddress, string? userAgent,
        Guid? userId, string? userDisplayName)
    {
        Id = Guid.NewGuid();
        HttpMethod = httpMethod;
        Path = path;
        QueryString = Truncate(queryString, 2000);
        RequestBody = Truncate(requestBody, 100_000);
        StatusCode = statusCode;
        ResponseBody = Truncate(responseBody, 100_000);
        DurationMs = durationMs;
        IpAddress = ipAddress;
        UserAgent = Truncate(userAgent, 500);
        UserId = userId;
        UserDisplayName = Truncate(userDisplayName, 100);
        CreatedAt = ChinaTime.Now;
    }

    private static string? Truncate(string? value, int maxLength) =>
        value != null && value.Length > maxLength ? value[..maxLength] : value;
}
