namespace RBS.Core.Common;

/// <summary>
/// 中国标准时间（UTC+8）工具类
/// </summary>
public static class ChinaTime
{
    private static readonly TimeZoneInfo ChinaTz =
        TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

    /// <summary>获取当前东八区时间</summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ChinaTz);

    /// <summary>获取当前东八区日期</summary>
    public static DateTime Today => Now.Date;
}
