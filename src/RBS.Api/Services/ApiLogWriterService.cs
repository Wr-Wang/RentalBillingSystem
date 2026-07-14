using System.Threading.Channels;
using Dapper;
using RBS.Api.Middleware;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Api.Services;

/// <summary>
/// API 日志批量写入服务 — 从 Channel 读取日志，每 50 条或 2 秒批量写入数据库
/// 使用多行 INSERT 减少数据库往返
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

            // 多行 INSERT — 一次往返写入全部
            var cols = "(Id, UserId, UserDisplayName, HttpMethod, ApiPath, QueryString, RequestBody, StatusCode, ResponseBody, DurationMs, ClientIp, UserAgent, RequestAt)";
            var values = string.Join(",",
                buffer.Select((_, i) => $"(@Id{i}, @UserId{i}, @UserDisplayName{i}, @HttpMethod{i}, @ApiPath{i}, @QueryString{i}, @RequestBody{i}, @StatusCode{i}, @ResponseBody{i}, @DurationMs{i}, @ClientIp{i}, @UserAgent{i}, @RequestAt{i})"));

            var parms = new DynamicParameters();
            for (int i = 0; i < buffer.Count; i++)
            {
                var log = buffer[i];
                parms.Add($"@Id{i}", log.Id);
                parms.Add($"@UserId{i}", log.UserId);
                parms.Add($"@UserDisplayName{i}", log.UserDisplayName);
                parms.Add($"@HttpMethod{i}", log.HttpMethod);
                parms.Add($"@ApiPath{i}", log.Path);
                parms.Add($"@QueryString{i}", log.QueryString);
                parms.Add($"@RequestBody{i}", log.RequestBody);
                parms.Add($"@StatusCode{i}", log.StatusCode);
                parms.Add($"@ResponseBody{i}", log.ResponseBody);
                parms.Add($"@DurationMs{i}", log.DurationMs);
                parms.Add($"@ClientIp{i}", log.IpAddress);
                parms.Add($"@UserAgent{i}", log.UserAgent);
                parms.Add($"@RequestAt{i}", log.RequestAt);
            }

            await conn.ExecuteAsync($"INSERT INTO ApiLogs {cols} VALUES {values}", parms);
        }
        catch { }
    }
}
