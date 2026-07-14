namespace RBS.Core.Common;

/// <summary>
/// 中国标准时间（CST, UTC+8）工具类
///
/// 职责：提供统一的东八区时间获取入口，确保全系统使用一致的中国标准时间，
/// 避免因服务器时区配置不同导致的时间偏差。
///
/// 设计说明：
/// - 所有需要记录时间戳的业务代码（创建时间、更新时间、事件发生时间等）
///   都应通过 ChinaTime.Now 获取，而非直接调用 DateTime.Now
/// - 内部使用 TimeZoneInfo.ConvertTimeFromUtc 进行转换，保证不受服务器本地时区影响
/// - 单元测试中可通过设置 DateTime.UtcNow 的 mock 来间接控制 ChinaTime.Now
///
/// 使用场景：
/// - AuditableEntity 构造时自动调用 ChinaTime.Now 设置 CreatedAt
/// - 所有领域事件构造时通过 ChinaTime.Now 设置 OccurredAt
/// - 业务逻辑中涉及中国时间的比较和计算
///
/// 注意：
/// - Windows 环境下时区 ID 为 "China Standard Time"
/// - Linux 环境下时区 ID 为 "Asia/Shanghai"（如部署环境不一致需调整）
/// </summary>
public static class ChinaTime
{
    /// <summary>
    /// 中国标准时区信息（UTC+8）
    /// Windows 时区 ID: "China Standard Time"
    /// Linux 时区 ID: "Asia/Shanghai"
    /// </summary>
    private static readonly TimeZoneInfo ChinaTz =
        TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

    /// <summary>
    /// 获取当前中国标准时间（东八区，UTC+8）
    /// 从 DateTime.UtcNow 转换而来，不受服务器本地时区配置影响。
    /// 返回的 DateTime.Kind 为 Unspecified。
    /// </summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ChinaTz);

    /// <summary>
    /// 获取当前中国标准日期（日期部分，时间归零）
    /// 用于只需要日期比较的场景（如按日期筛选数据）。
    /// </summary>
    public static DateTime Today => Now.Date;
}
