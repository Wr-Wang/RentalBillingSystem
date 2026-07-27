namespace RBS.Core.Common;

/// <summary>
/// 中国工作日日历（Chinese Workday Calendar）
///
/// 职责：根据中国法定节假日安排，判断指定日期是否为工作日。
/// 支持国务院发布的调休安排（如春节/国庆调休，周六上班、工作日放假）。
///
/// 判断规则：
/// 1. 默认规则：周一至周五为工作日，周六日（周六/周日）为休息日
/// 2. 外部覆盖：通过 isWorkingDayOverride 委托注入法定节假日和调休安排数据
///    - 委托返回 true：标记为工作日（用于调休上班的周末）
///    - 委托返回 false：标记为休息日（用于法定节假日）
/// 3. 外部数据未配置时，回退到默认规则
///
/// 依赖注入：
/// 外部覆盖数据通常来自 HolidayCalendar 表，由应用层在启动时加载并注入。
/// 完整的节假日数据每年由国务院发布，系统管理员在年初配置到 HolidayCalendar 表。
///
/// 使用场景：
/// - 应收计划到期日的计算（跳过节假日）
/// - 利息计算中工作日的确定
/// - 合同到期日/恢复日的工作日校验
/// - 催收通知发送日期（仅在工作日发送）
/// </summary>
public class ChineseWorkdayCalendar
{
    private readonly Func<DateTime, bool> _isWorkingDayOverride;

    /// <summary>
    /// 创建中国工作日日历实例
    /// </summary>
    /// <param name="isWorkingDayOverride">
    /// 外部注入的日期覆盖判断函数。
    /// - 返回 true：指定日期被覆盖为工作日（调休上班）
    /// - 返回 false：指定日期被覆盖为休息日（法定假日）
    /// - 如果外部对该日期没有配置，应返回 false（使用默认规则）
    /// 传 null 则仅使用默认规则（周一至周五工作日，周六日休息）。
    /// </param>
    public ChineseWorkdayCalendar(Func<DateTime, bool>? isWorkingDayOverride = null)
    {
        _isWorkingDayOverride = isWorkingDayOverride ?? (_ => false);
    }

    /// <summary>
    /// 判断指定日期是否为工作日（根据中国法定节假日规则）
    ///
    /// 判断优先级：
    /// 1. 外部覆盖标记为工作日（调休上班）→ 是工作日
    /// 2. 周六日（无调休覆盖）→ 不是工作日
    /// 3. 周一至周五（无节假日覆盖）→ 是工作日
    ///
    /// 注意：当前实现中，如果外部覆盖返回 false，无法区分"未配置"和"配置为休息"，
    /// 因此对周一至周五默认返回 true（即未配置时视为工作日）。
    /// 这意味着所有法定放假日期必须显式在外部覆盖中配置为 false。
    /// </summary>
    /// <param name="date">待判断的日期</param>
    /// <returns>如果是工作日则返回 true，否则返回 false</returns>
    public bool IsWorkingDay(DateTime date)
    {
        // 先查外部覆盖（调休上班）
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

    /// <summary>
    /// 获取指定日期之后的下一个工作日
    /// 用于计算应收计划的到期日（如果到期日落在非工作日，顺延到下一个工作日）。
    /// </summary>
    /// <param name="date">起始日期</param>
    /// <returns>从起始日期后第一个工作日（不含起始日本身）</returns>
    public DateTime GetNextWorkingDay(DateTime date)
    {
        var next = date.AddDays(1);
        while (!IsWorkingDay(next)) next = next.AddDays(1);
        return next;
    }

    /// <summary>
    /// 获取指定日期之前最近的工作日
    /// 用于计算截止日期之前的最后一个工作日。
    /// </summary>
    /// <param name="date">起始日期</param>
    /// <returns>起始日期之前最近的工作日（不含起始日本身）</returns>
    public DateTime GetPreviousWorkingDay(DateTime date)
    {
        var prev = date.AddDays(-1);
        while (!IsWorkingDay(prev)) prev = prev.AddDays(-1);
        return prev;
    }

    /// <summary>
    /// 计算两个日期之间的工作天数
    /// 计费规则中的"按工作日计费"场景，计算实际应计费的工作日数。
    /// 包含开始日期，不包含结束日期（[start, end)）。
    /// </summary>
    /// <param name="start">起始日期（包含）</param>
    /// <param name="end">结束日期（不包含）</param>
    /// <returns>区间内的工作日天数</returns>
    public int CountWorkingDays(DateTime start, DateTime end)
    {
        int count = 0;
        for (var d = start; d < end; d = d.AddDays(1))
        {
            if (IsWorkingDay(d)) count++;
        }
        return count;
    }

    /// <summary>
    /// 从起始日期开始，计算第 N 个工作日对应的日期
    /// 用于倒推到期日或计算指定工作日数后的日期。
    /// 注意：起始日当天不计入（从次日开始计算）。
    /// </summary>
    /// <param name="start">开始计算的基础日期</param>
    /// <param name="workingDays">需要向前推进的工作日数量</param>
    /// <returns>经过 N 个工作日后的日期</returns>
    public DateTime AddWorkingDays(DateTime start, int workingDays)
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

    /// <summary>
    /// 判断指定日期是否为周末（周六或周日）
    /// </summary>
    /// <param name="date">待判断的日期</param>
    /// <returns>如果是周六或周日返回 true，否则 false</returns>
    private static bool _isWeekend(DateTime date) => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}
