namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

public interface IFeeCodeRepository : IRepository<FeeCode>
{
    Task<FeeCode?> GetByCodeAsync(string code, Guid companyId, CancellationToken ct = default);
    Task<List<FeeCode>> GetByCategoryAsync(string category, Guid companyId, CancellationToken ct = default);
}
