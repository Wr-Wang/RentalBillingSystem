namespace RBS.Core.Interfaces.Services;

/// <summary>
/// 客户端信息服务 — 从 HTTP 请求中获取客户端 IP 和主机名
/// </summary>
public interface IClientInfoService
{
    /// <summary>获取客户端 IP 地址</summary>
    string? GetClientIp();

    /// <summary>获取客户端主机名</summary>
    string? GetClientHostname();
}
