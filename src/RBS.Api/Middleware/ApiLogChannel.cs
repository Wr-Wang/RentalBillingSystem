using System.Threading.Channels;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Api.Middleware;

/// <summary>
/// API 日志共享通道（Singleton）— 中间件写入，后台服务批量读取写入 DB
/// </summary>
public class ApiLogChannel
{
    public Channel<ApiLog> LogChannel { get; } = System.Threading.Channels.Channel.CreateBounded<ApiLog>(
        new BoundedChannelOptions(2000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleWriter = false,
            SingleReader = true
        });
}
