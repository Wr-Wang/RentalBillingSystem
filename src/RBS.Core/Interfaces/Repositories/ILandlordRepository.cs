namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

public interface ILandlordRepository : IRepository<Landlord>
{
    Task<Landlord?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<Landlord>> GetActiveAsync(CancellationToken ct = default);
}
