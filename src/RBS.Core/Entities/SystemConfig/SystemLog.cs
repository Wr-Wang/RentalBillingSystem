namespace RBS.Core.Entities.SystemConfig;

/// <summary>
/// 系统异常日志
/// </summary>
public class SystemLog
{
    public Guid Id { get; private set; }
    public string Level { get; private set; } = "Error";
    public string? Message { get; private set; }
    public string? Exception { get; private set; }
    public string? Source { get; private set; }
    public string? Path { get; private set; }
    public string? Method { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserDisplayName { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SystemLog() { }

    public SystemLog(string level, string? message, string? exception, string? source,
        string? path = null, string? method = null, string? ip = null,
        string? userAgent = null, Guid? userId = null, string? userDisplayName = null)
    {
        Id = Guid.NewGuid();
        Level = level;
        Message = message;
        Exception = exception;
        Source = source;
        Path = path;
        Method = method;
        IpAddress = ip;
        UserAgent = userAgent;
        UserId = userId;
        UserDisplayName = userDisplayName;
        CreatedAt = DateTime.UtcNow;
    }
}
