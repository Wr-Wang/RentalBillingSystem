using RBS.Core.Entities.Accounting;

namespace RBS.Application.Common.Interfaces;

public interface IAccountingPeriodService
{
    Task<List<AccountingPeriod>> GetAllAsync(CancellationToken ct = default);
    Task<AccountingPeriod> OpenPeriodAsync(string period, CancellationToken ct = default);
    Task ClosePeriodAsync(Guid id, CancellationToken ct = default);
    Task ReopenPeriodAsync(Guid id, CancellationToken ct = default);
    Task LockPeriodAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsPeriodOpenAsync(string period, CancellationToken ct = default);
}
