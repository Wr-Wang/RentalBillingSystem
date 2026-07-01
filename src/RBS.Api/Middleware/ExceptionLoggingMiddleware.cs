using System.Security.Claims;
using RBS.Core.Common;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Api.Middleware;

public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

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
