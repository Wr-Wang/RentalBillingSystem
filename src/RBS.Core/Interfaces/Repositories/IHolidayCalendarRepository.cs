namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.SystemConfig;

/// <summary>
/// 节假日日历仓储接口。
/// 定义节假日日历聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供按年份和按日期查询节假日的业务方法。
/// 用于租金到期日自动顺延等业务场景。
/// </summary>
public interface IHolidayCalendarRepository : IRepository<HolidayCalendar>
{
    /// <summary>
    /// 获取指定公司指定年份的所有节假日列表。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="year">年份</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>节假日列表</returns>
    Task<List<HolidayCalendar>> GetByYearAsync(Guid companyId, int year, CancellationToken ct = default);

    /// <summary>
    /// 获取指定公司指定日期的节假日信息。
    /// 用于判断某天是否为节假日（如到期日顺延判断）。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="date">日期</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>节假日实体，非节假日时返回 null</returns>
    Task<HolidayCalendar?> GetByDateAsync(Guid companyId, DateOnly date, CancellationToken ct = default);
}
