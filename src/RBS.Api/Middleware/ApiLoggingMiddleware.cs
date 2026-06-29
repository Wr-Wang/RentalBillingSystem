using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Api.Middleware;

/// <summary>
/// API 调用日志中间件 — 记录每次请求的完整上下文到 Channel，由后台服务批量写入 DB
/// </summary>
public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiLogChannel _logChannel;

    public ApiLoggingMiddleware(RequestDelegate next, ApiLogChannel logChannel)
    {
        _next = next;
        _logChannel = logChannel;
    }

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
