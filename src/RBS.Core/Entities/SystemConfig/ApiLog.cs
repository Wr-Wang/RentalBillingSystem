namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Common;

/// <summary>
/// API 调用日志 — 记录每次请求的完整上下文
/// 用于接口调用审计、性能监控和问题排查。
/// 包含用户信息、请求详情、响应信息、网络信息及耗时统计，
/// 长文本字段在持久化时做了截断处理以防止数据溢出
/// </summary>
public class ApiLog
{
    /// <summary>
    /// 日志唯一标识
    /// </summary>
    public Guid Id { get; private set; }

    // ===== 用户信息 =====

    /// <summary>
    /// 操作用户标识（可选），未登录请求为 null
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// 操作用户显示名称（可选），便于日志查询识别
    /// </summary>
    public string? UserDisplayName { get; private set; }

    // ===== 请求信息 =====

    /// <summary>
    /// HTTP 请求方法，如 "GET"、"POST"、"PUT"、"DELETE"
    /// </summary>
    public string HttpMethod { get; private set; } = string.Empty;

    /// <summary>
    /// 请求路径，如 "/api/v1/contracts/list"，不含查询参数
    /// </summary>
    public string Path { get; private set; } = string.Empty;

    /// <summary>
    /// 查询字符串（可选），URL 中 "?" 后的参数部分，最长 2000 字符
    /// </summary>
    public string? QueryString { get; private set; }

    /// <summary>
    /// 请求体内容（可选），POST/PUT 请求的 JSON 数据，最长 100000 字符
    /// </summary>
    public string? RequestBody { get; private set; }

    // ===== 响应信息 =====

    /// <summary>
    /// HTTP 状态码，如 200（成功）、400（参数错误）、500（服务器错误）
    /// </summary>
    public int StatusCode { get; private set; }

    /// <summary>
    /// 响应体内容（可选），响应的 JSON 数据，最长 100000 字符
    /// </summary>
    public string? ResponseBody { get; private set; }

    /// <summary>
    /// 请求处理总耗时（毫秒），从收到请求到返回响应的完整耗时
    /// </summary>
    public long DurationMs { get; private set; }

    // ===== 网络信息 =====

    /// <summary>
    /// 客户端 IP 地址（可选），用于来源追踪和安全审计
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// 客户端 UserAgent（可选），用于识别客户端类型和版本，最长 500 字符
    /// </summary>
    public string? UserAgent { get; private set; }

    // ===== 时间 =====

    /// <summary>
    /// 请求接收时间（北京时间）
    /// </summary>
    public DateTime RequestAt { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ApiLog() { }

    /// <summary>
    /// 创建 API 日志实例。自动生成 Id 并记录请求时间（北京时间）。
    /// 长文本字段自动截断以防止数据库溢出
    /// </summary>
    /// <param name="httpMethod">HTTP 方法</param>
    /// <param name="path">请求路径</param>
    /// <param name="queryString">查询字符串（最长保留 2000 字符）</param>
    /// <param name="requestBody">请求体（最长保留 100000 字符）</param>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <param name="responseBody">响应体（最长保留 100000 字符）</param>
    /// <param name="durationMs">耗时（毫秒）</param>
    /// <param name="ipAddress">客户端 IP</param>
    /// <param name="userAgent">客户端 UserAgent（最长保留 500 字符）</param>
    /// <param name="userId">操作用户标识</param>
    /// <param name="userDisplayName">操作用户显示名称（最长保留 100 字符）</param>
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
        RequestAt = ChinaTime.Now;
    }

    /// <summary>
    /// 截断字符串到指定最大长度，防止存储溢出
    /// </summary>
    /// <param name="value">原始字符串</param>
    /// <param name="maxLength">最大允许长度</param>
    /// <returns>截断后的字符串，超长时截取前 maxLength 字符</returns>
    private static string? Truncate(string? value, int maxLength) =>
        value != null && value.Length > maxLength ? value[..maxLength] : value;
}
