namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Contract;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task<List<Tenant>> SearchAsync(string keyword, CancellationToken ct = default);
}
