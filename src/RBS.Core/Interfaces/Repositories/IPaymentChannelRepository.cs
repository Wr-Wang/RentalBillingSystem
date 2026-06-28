namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

public interface IPaymentChannelRepository : IRepository<PaymentChannel>
{
    Task<List<PaymentChannel>> GetActiveByCompanyAsync(Guid companyId, CancellationToken ct = default);
}
