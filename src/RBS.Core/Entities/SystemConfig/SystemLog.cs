namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Common;

/// <summary>
/// 系统异常/事件日志 — 记录系统运行过程中的异常及重要事件
/// 与 ApiLog 不同，SystemLog 侧重于记录后端服务异常、业务规则冲突和系统级事件，
/// 用于运维监控、故障排查和审计追溯
/// </summary>
public class SystemLog
{
    /// <summary>
    /// 日志唯一标识
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 日志级别：Error=错误, Warning=警告, Info=信息, Debug=调试
    /// 默认值为 "Error"
    /// </summary>
    public string Level { get; private set; } = "Error";

    /// <summary>
    /// 日志消息（可选），简要描述事件或异常的内容
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// 异常详情（可选），异常的堆栈信息或完整消息
    /// </summary>
    public string? Exception { get; private set; }

    /// <summary>
    /// 来源组件（可选），产生日志的模块或服务名称，如 "RentCalculationService"
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    /// 请求路径（可选），发生异常时的 API 路径或页面地址
    /// </summary>
    public string? Path { get; private set; }

    /// <summary>
    /// 请求方法（可选），发生异常时的 HTTP 方法或业务方法名称
    /// </summary>
    public string? Method { get; private set; }

    /// <summary>
    /// 客户端 IP 地址（可选），用于来源追踪
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// 客户端 UserAgent（可选），用于识别客户端类型
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// 关联用户标识（可选），异常发生时正在操作的用户
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// 关联用户显示名称（可选），便于日志查询识别
    /// </summary>
    public string? UserDisplayName { get; private set; }

    /// <summary>
    /// 日志记录时间（北京时间）
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private SystemLog() { }

    /// <summary>
    /// 创建系统日志实例。自动生成 Id 并记录创建时间（北京时间）
    /// </summary>
    /// <param name="level">日志级别（Error/Warning/Info/Debug）</param>
    /// <param name="message">日志消息（可选）</param>
    /// <param name="exception">异常详情（可选）</param>
    /// <param name="source">来源组件（可选）</param>
    /// <param name="path">请求路径（可选）</param>
    /// <param name="method">请求方法（可选）</param>
    /// <param name="ip">客户端 IP（可选）</param>
    /// <param name="userAgent">客户端 UserAgent（可选）</param>
    /// <param name="userId">关联用户标识（可选）</param>
    /// <param name="userDisplayName">关联用户名称（可选）</param>
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
        CreatedAt = ChinaTime.Now;
    }
}
