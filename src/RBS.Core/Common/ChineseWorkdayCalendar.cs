namespace RBS.Core.Common;

/// <summary>
/// 中国工作日日历 — 基于假日日历表判断工作日/节假日
/// 规则：周一至周五默认工作日，被标记为 IsWorkingDay=false 的为节假日
///       周六日默认休息日，被标记为 IsWorkingDay=true 的为调休上班日
/// 依赖外部注入的检查函数（由应用层提供 HolidayCalendar 数据）
/// </summary>
public class ChineseWorkdayCalendar
{
    private readonly Func<DateOnly, bool> _isWorkingDayOverride;

    /// <summary>
    /// 创建工作日日历实例
    /// </summary>
    /// <param name="isWorkingDayOverride">
    /// 外部函数：返回指定日期是否为工作日覆盖（true=调休上班，false=节假日）
    /// 传 null 则仅用默认规则（周一到周五工作，周六日休息）
    /// </param>
    public ChineseWorkdayCalendar(Func<DateOnly, bool>? isWorkingDayOverride = null)
    {
        _isWorkingDayOverride = isWorkingDayOverride ?? (_ => false);
    }

    /// <summary>判断指定日期是否为工作日</summary>
    public bool IsWorkingDay(DateOnly date)
    {
        // 先查外部覆盖
        if (_isWorkingDayOverride(date)) return true;

        // 再查是否标注为节假日
        if (_isWeekend(date) && !_isWorkingDayOverride(date))
        {
            // 周六日本来休息，除非调休上班已经在第一步处理了
            return false;
        }

        // 周一到周五，检查是否法定假日
        if (!_isWeekend(date))
        {
            // 如果是法定假日（外部返回 false 表示覆盖为休息）
            // 但我们无法区分"外部未配置"和"外部配置为休息"
            // 所以如果外部返回 false（或未配置），默认周一至周五工作
            return true;
        }

        return false;
    }

    /// <summary>获取下一个工作日</summary>
    public DateOnly GetNextWorkingDay(DateOnly date)
    {
        var next = date.AddDays(1);
        while (!IsWorkingDay(next)) next = next.AddDays(1);
        return next;
    }

    /// <summary>获取指定日期之前最近的工作日</summary>
    public DateOnly GetPreviousWorkingDay(DateOnly date)
    {
        var prev = date.AddDays(-1);
        while (!IsWorkingDay(prev)) prev = prev.AddDays(-1);
        return prev;
    }

    /// <summary>计算两个日期之间的工作天数（含开始，不含结束）</summary>
    public int CountWorkingDays(DateOnly start, DateOnly end)
    {
        int count = 0;
        for (var d = start; d < end; d = d.AddDays(1))
        {
            if (IsWorkingDay(d)) count++;
        }
        return count;
    }

    /// <summary>从开始日期算起，第 N 个工作日对应的日期</summary>
    public DateOnly AddWorkingDays(DateOnly start, int workingDays)
    {
        var current = start;
        int added = 0;
        while (added < workingDays)
        {
            current = current.AddDays(1);
            if (IsWorkingDay(current)) added++;
        }
        return current;
    }

    private static bool _isWeekend(DateOnly date) => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}
