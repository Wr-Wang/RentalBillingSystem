using Microsoft.AspNetCore.Http;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 客户端信息服务实现 — 从 IHttpContextAccessor 获取请求端 IP 和主机名
/// </summary>
public class ClientInfoService : IClientInfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientInfoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取客户端 IP 地址
    /// </summary>
    public string? GetClientIp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Connection.RemoteIpAddress == null) return null;
        var ip = context.Connection.RemoteIpAddress.ToString();
        return ip == "::1" ? "127.0.0.1" : ip;
    }

    /// <summary>
    /// 获取客户端主机名（当前返回 IP，后续可扩展为反向 DNS 查询）
    /// </summary>
    public string? GetClientHostname()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Connection.RemoteIpAddress == null) return null;
        var ip = context.Connection.RemoteIpAddress.ToString();
        return ip == "::1" ? "localhost" : ip;
    }
}
