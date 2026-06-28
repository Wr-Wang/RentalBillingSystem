using System.Security.Claims;
using System.Text.Json;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Api.Middleware;

/// <summary>
/// 全局异常捕获中间件 — 自动记录未处理异常到 SystemLogs 表
/// </summary>
public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public ExceptionLoggingMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await LogExceptionAsync(context, ex);
            // 返回统一错误响应
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = "服务器内部错误",
                errorId = Guid.NewGuid()
            }));
        }
    }

    private async Task LogExceptionAsync(HttpContext context, Exception ex)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.AppDbContext>();

            var userIdStr = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = userIdStr != null && Guid.TryParse(userIdStr, out var uid) ? uid : null;

            var displayName = context.User?.FindFirst("DisplayName")?.Value;

        var log = new SystemLog(
                level: "Error",
                message: ex.Message,
                exception: ex.ToString(),
                source: ex.Source,
                path: context.Request.Path,
                method: context.Request.Method,
                ip: context.Connection.RemoteIpAddress?.ToString(),
                userAgent: context.Request.Headers["User-Agent"],
                userId: userId,
                userDisplayName: displayName
            );

            db.SystemLogs.Add(log);
            await db.SaveChangesAsync();
        }
        catch
        {
            // 日志写入失败不应影响主流程
        }
    }
}
