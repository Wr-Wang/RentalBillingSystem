namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

public interface IReceivablePlanRepository : IRepository<ReceivablePlan>
{
    Task<List<ReceivablePlan>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task<ReceivablePlan?> GetByContractPeriodFeeAsync(Guid contractId, string period, Guid feeCodeId, CancellationToken ct = default);
    Task<List<ReceivablePlan>> GetOverdueAsync(Guid landlordId, CancellationToken ct = default);
}
