namespace RBS.Application.Common.Interfaces;

/// <summary>报表服务 — 将所有报表查询逻辑从控制器抽取到此服务</summary>
public interface IReportingService
{
    Task<object> GetCollectionRateAsync(string? period, CancellationToken ct);
    Task<object> GetOverdueDetailAsync(Guid? companyId, string? period, CancellationToken ct);
    Task<object> GetDailyReceiptAsync(DateOnly? date, CancellationToken ct);
    Task<object> GetMonthlyReceiptAsync(string? period, CancellationToken ct);
    Task<object> GetFeeRevenueAsync(string? period, CancellationToken ct);
    Task<object> GetOccupancyRateAsync(string? period, CancellationToken ct);
}
