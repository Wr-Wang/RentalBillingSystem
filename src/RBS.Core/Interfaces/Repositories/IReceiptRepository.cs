namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

public interface IReceiptRepository : IRepository<Receipt>
{
    Task<List<Receipt>> GetPendingConfirmAsync(Guid companyId, CancellationToken ct = default);
    Task<decimal> GetTotalConfirmedAsync(Guid contractId, CancellationToken ct = default);
}
