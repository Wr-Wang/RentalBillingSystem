using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Api.Middleware;

/// <summary>
/// API 调用日志中间件 — 记录每次请求的完整上下文到 Channel，由后台服务批量写入 DB
/// </summary>
/// <remarks>
/// 功能特性：
/// <list type="bullet">
///   <item><description>自动跳过 /api/apilogs 和 /api/swagger 路径，避免递归日志</description></item>
///   <item><description>读取请求 Body（限制 200KB 以内，避免大文件日志）</description></item>
///   <item><description>劫持响应 Body 流（同样限制 200KB）</description></item>
///   <item><description>记录用户信息（从 JWT Claims 中提取 userId 和 displayName）</description></item>
///   <item><description>使用 Channel&lt;ApiLog&gt; 非阻塞写入，不阻塞请求处理</description></item>
///   <item><description>响应完成后在 finally 块中归还原始 Body 流</description></item>
/// </list>
/// 设计模式：Middleware Pipeline 中的日志记录层 + Producer-Consumer（Channel）。
/// 消费端：ApiLogWriterService（BackgroundService）。
/// </remarks>
public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiLogChannel _logChannel;

    /// <summary>
    /// 初始化 API 日志中间件
    /// </summary>
    /// <param name="next">下一个中间件委托</param>
    /// <param name="logChannel">API 日志共享通道（单例）</param>
    public ApiLoggingMiddleware(RequestDelegate next, ApiLogChannel logChannel)
    {
        _next = next;
        _logChannel = logChannel;
    }

    /// <summary>
    /// 执行中间件 — 记录完整请求/响应上下文并通过 Channel 发送
    /// </summary>
    /// <remarks>
    /// 流程：
    /// 1. 跳过 ApiLogs 和 Swagger 接口
    /// 2. 读取请求 Body（缓冲区模式，归还流给下游）
    /// 3. 劫持响应 Body（替换为 MemoryStream）
    /// 4. 执行下游中间件
    /// 5. finally 中读取响应 Body、归还原始流
    /// 6. 构造 ApiLog 并写入 Channel（非阻塞）
    /// </remarks>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // 跳过 ApiLogs 自身接口，避免递归
        if (context.Request.Path.StartsWithSegments("/api/apilogs", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.StartsWithSegments("/api/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        // ===== 读取请求 Body =====
        context.Request.EnableBuffering();
        string? requestBody = null;
        if (context.Request.ContentLength > 0 && context.Request.ContentLength < 200_000) // < 200KB
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // 归还流给下游
        }

        // ===== 劫持响应 Body =====
        var originalBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // ===== 读取响应 Body =====
            string? responseBody = null;
            if (responseBodyStream.Length < 200_000) // < 200KB
            {
                responseBodyStream.Position = 0;
                responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            }

            // 归还响应流
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            // ===== 提取用户信息 =====
            var userIdStr = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = userIdStr != null && Guid.TryParse(userIdStr, out var uid) ? uid : null;
            var displayName = context.User?.FindFirst("DisplayName")?.Value;

            // ===== 构造日志 =====
            var log = new ApiLog(
                httpMethod: context.Request.Method,
                path: context.Request.Path,
                queryString: context.Request.QueryString.ToString(),
                requestBody: requestBody,
                statusCode: context.Response.StatusCode,
                responseBody: responseBody,
                durationMs: stopwatch.ElapsedMilliseconds,
                ipAddress: context.Connection.RemoteIpAddress?.ToString(),
                userAgent: context.Request.Headers["User-Agent"],
                userId: userId,
                userDisplayName: displayName
            );

            // 写入通道（非阻塞）
            _logChannel.LogChannel.Writer.TryWrite(log);
        }
    }
}
