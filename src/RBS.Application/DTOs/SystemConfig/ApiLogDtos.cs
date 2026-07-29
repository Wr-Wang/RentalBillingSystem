namespace RBS.Application.DTOs.SystemConfig;

/// <summary>
/// API 日志列表项 DTO — 仅包含列表展示需要的字段，排除 RequestBody/ResponseBody 大文本
/// </summary>
public class ApiLogListItemDto
{
    /// <summary>总记录数（COUNT(*) OVER() 窗口函数映射）</summary>
    public int Total { get; set; }
    public Guid Id { get; set; }
    public string? HttpMethod { get; set; }
    public string? ApiPath { get; set; }
    public int StatusCode { get; set; }
    public int DurationMs { get; set; }
    public string? ClientIp { get; set; }
    public Guid? UserId { get; set; }
    public DateTime RequestAt { get; set; }
}

/// <summary>
/// API 日志详情 DTO — 包含完整请求/响应数据
/// </summary>
public class ApiLogDetailDto
{
    public Guid Id { get; set; }
    public string? HttpMethod { get; set; }
    public string? ApiPath { get; set; }
    public string? QueryString { get; set; }
    public string? RequestBody { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public long DurationMs { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public Guid? UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public DateTime RequestAt { get; set; }
}
