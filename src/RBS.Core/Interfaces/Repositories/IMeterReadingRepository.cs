namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

public interface IMeterReadingRepository : IRepository<MeterReading>
{
    Task<MeterReading?> GetLatestReadingAsync(Guid contractFeeConfigId, CancellationToken ct = default);
    Task<List<MeterReading>> GetHistoryAsync(Guid contractFeeConfigId, int year, int month, CancellationToken ct = default);
    Task<bool> ReadingExistsAsync(Guid contractFeeConfigId, int year, int month, CancellationToken ct = default);
}
