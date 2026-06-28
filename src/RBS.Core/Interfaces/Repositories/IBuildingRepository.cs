namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Property;

public interface IBuildingRepository : IRepository<Building>
{
    Task<List<Building>> GetByCompanyIdAsync(Guid companyId, CancellationToken ct = default);
}
