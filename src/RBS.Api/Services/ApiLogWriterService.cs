using System.Threading.Channels;
using RBS.Core.Entities.SystemConfig;
using Dapper;
using RBS.Api.Middleware;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Api.Services;

/// <summary>
/// API 日志批量写入服务 — 从 Channel 读取日志，每 50 条或 2 秒批量写入数据库
/// </summary>
public class ApiLogWriterService : BackgroundService
{
    private readonly Channel<ApiLog> _channel;
    private readonly IServiceProvider _serviceProvider;

    public ApiLogWriterService(ApiLogChannel logChannel, IServiceProvider serviceProvider)
    {
        _channel = logChannel.LogChannel;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<ApiLog>(capacity: 50);

        while (!stoppingToken.IsCancellationRequested)
        {
            buffer.Clear();

            try
            {
                var first = await _channel.Reader.ReadAsync(stoppingToken);
                buffer.Add(first);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                try
                {
                    while (buffer.Count < 50)
                    {
                        var item = await _channel.Reader.ReadAsync(cts.Token);
                        buffer.Add(item);
                    }
                }
                catch (OperationCanceledException) { }

                await FlushAsync(buffer, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                while (_channel.Reader.TryRead(out var remaining))
                    buffer.Add(remaining);
                if (buffer.Count > 0)
                    await FlushAsync(buffer, stoppingToken);
                break;
            }
            catch { }
        }
    }

    private async Task FlushAsync(List<ApiLog> buffer, CancellationToken ct)
    {
        if (buffer.Count == 0) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            using var conn = db.CreateConnection();
            conn.Open();

            foreach (var log in buffer)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO ApiLogs (Id, UserId, UserDisplayName, HttpMethod, Path, QueryString, RequestBody, StatusCode, ResponseBody, DurationMs, IpAddress, UserAgent, CreatedAt)
                    VALUES (@Id, @UserId, @UserDisplayName, @HttpMethod, @Path, @QueryString, @RequestBody, @StatusCode, @ResponseBody, @DurationMs, @IpAddress, @UserAgent, @CreatedAt)",
                    new
                    {
                        log.Id, log.UserId, log.UserDisplayName, log.HttpMethod, log.Path,
                        log.QueryString, log.RequestBody, log.StatusCode, log.ResponseBody,
                        log.DurationMs, log.IpAddress, log.UserAgent, log.CreatedAt
                    });
            }
        }
        catch { }
    }
}
