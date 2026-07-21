namespace RBS.Application.Common.Interfaces;

/// <summary>报表服务 — 将所有报表查询逻辑从控制器抽取到此服务</summary>
public interface IReportingService
{
    /// <summary>
    /// 获取收款率统计
    /// </summary>
    /// <param name="companyId">公司 ID，null 表示全部公司</param>
    /// <param name="period">账期 (yyyy-MM)，null 表示当前月</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>收款率统计数据</returns>
    Task<object> GetCollectionRateAsync(Guid? companyId, string? period, CancellationToken ct);

    /// <summary>
    /// 获取逾期明细
    /// </summary>
    /// <param name="companyId">公司 ID，null 表示全部公司</param>
    /// <param name="period">账期 (yyyy-MM)</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>逾期明细数据</returns>
    Task<object> GetOverdueDetailAsync(Guid? companyId, string? period, CancellationToken ct);

    /// <summary>
    /// 获取每日收款汇总
    /// </summary>
    /// <param name="companyId">公司 ID，null 表示全部公司</param>
    /// <param name="date">日期，null 表示今日</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>每日收款数据</returns>
    Task<object> GetDailyReceiptAsync(Guid? companyId, DateOnly? date, CancellationToken ct);

    /// <summary>
    /// 获取月度收款汇总
    /// </summary>
    /// <param name="companyId">公司 ID，null 表示全部公司</param>
    /// <param name="period">账期 (yyyy-MM)，null 表示当前月</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>月度收款数据</returns>
    Task<object> GetMonthlyReceiptAsync(Guid? companyId, string? period, CancellationToken ct);

    /// <summary>
    /// 获取费用收入统计
    /// </summary>
    /// <param name="period">账期 (yyyy-MM)，null 表示当前月</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>费用收入数据</returns>
    Task<object> GetFeeRevenueAsync(string? period, CancellationToken ct);

    /// <summary>
    /// 获取出租率统计
    /// </summary>
    /// <param name="period">账期 (yyyy-MM)，null 表示当前月</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>出租率数据</returns>
    Task<object> GetOccupancyRateAsync(string? period, CancellationToken ct);
}
