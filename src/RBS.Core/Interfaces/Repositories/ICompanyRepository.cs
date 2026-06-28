namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<Company>> GetActiveAsync(CancellationToken ct = default);
}
