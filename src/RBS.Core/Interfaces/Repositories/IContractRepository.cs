namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Contract;

public interface IContractRepository : IRepository<Contract>
{
    Task<Contract?> GetByContractNoAsync(string contractNo, CancellationToken ct = default);
    Task<List<Contract>> GetActiveContractsAsync(Guid companyId, CancellationToken ct = default);
    Task<List<Contract>> GetContractsExpiringAsync(DateOnly date, CancellationToken ct = default);
}
