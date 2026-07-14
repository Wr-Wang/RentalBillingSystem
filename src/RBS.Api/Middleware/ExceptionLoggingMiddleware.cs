using System.Security.Claims;
using RBS.Core.Common;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Api.Middleware;

/// <summary>
/// 全局异常捕获中间件 — 捕获所有未处理异常，记录到 SystemLogs 表
/// </summary>
/// <remarks>
/// 处理策略：
/// <list type="bullet">
///   <item><description>OperationCanceledException → 返回 HTTP 499（客户端取消）</description></item>
///   <item><description>其他异常 → 记录日志、返回统一 JSON 错误（code=SYSTEM_ERROR）</description></item>
///   <item><description>异常异步写入 SystemLogs 表（独立提交，失败静默）</description></item>
///   <item><description>中间件位于管道最前端，确保捕获所有下游异常</description></item>
/// </list>
/// 设计模式：Middleware Pipeline 中的全局异常处理层。
/// </remarks>
public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    /// <summary>
    /// 初始化异常捕获中间件
    /// </summary>
    /// <param name="next">下一个中间件委托</param>
    /// <param name="serviceProvider">服务提供器（用于创建 DI 作用域写日志）</param>
    /// <param name="logger">日志记录器</param>
    public ExceptionLoggingMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 执行中间件 — 捕获未处理异常并写入 SystemLogs
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP {Method} {Path} 产生未处理异常", context.Request?.Method, context.Request?.Path);
            await LogExceptionAsync(context, ex);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                code = "SYSTEM_ERROR",
                message = "系统错误，请稍后重试"
            }));
        }
    }

    private async Task LogExceptionAsync(HttpContext context, Exception ex)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            using var conn = db.CreateConnection();
            conn.Open();

            var userIdStr = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = userIdStr != null && Guid.TryParse(userIdStr, out var uid) ? uid : null;
            var displayName = context.User?.FindFirst("DisplayName")?.Value;

            await conn.ExecuteAsync(@"
                INSERT INTO SystemLogs (Id, Level, Message, Exception, Source, Path, Method, IpAddress, UserAgent, UserId, UserDisplayName, CreatedAt)
                VALUES (@Id, @Level, @Message, @Exception, @Source, @Path, @Method, @IpAddress, @UserAgent, @UserId, @UserDisplayName, @CreatedAt)",
                new
                {
                    Id = Guid.NewGuid(), Level = "Error", Message = ex.Message,
                    Exception = ex.ToString(), Source = ex.Source,
                    Path = context.Request.Path.Value, Method = context.Request.Method,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    UserId = userId, UserDisplayName = displayName,
                    CreatedAt = ChinaTime.Now
                });
        }
        catch
        {
            // 日志写入失败不应影响主流程
        }
    }
}
