using System.Threading.Channels;
using RBS.Api.Middleware;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data;

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
                // 等待第一条数据
                var first = await _channel.Reader.ReadAsync(stoppingToken);
                buffer.Add(first);

                // 2 秒内尽可能收满 50 条
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
                catch (OperationCanceledException)
                {
                    // 超时或取消，用当前积累的条数写入
                }

                // 批量写入数据库
                await FlushAsync(buffer, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // 应用关闭，把剩余日志写完
                while (_channel.Reader.TryRead(out var remaining))
                {
                    buffer.Add(remaining);
                }
                if (buffer.Count > 0)
                    await FlushAsync(buffer, stoppingToken);
                break;
            }
            catch
            {
                // 单次写入失败不中断循环
            }
        }
    }

    private async Task FlushAsync(List<ApiLog> buffer, CancellationToken ct)
    {
        if (buffer.Count == 0) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ApiLogs.AddRange(buffer);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // 日志写入失败不应影响应用程序
        }
    }
}
