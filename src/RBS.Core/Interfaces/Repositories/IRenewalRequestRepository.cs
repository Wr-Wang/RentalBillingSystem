using RBS.Core.Entities.Contract;

namespace RBS.Core.Interfaces.Repositories;

/// <summary>
/// 续签请求仓储接口
/// </summary>
public interface IRenewalRequestRepository : IRepository<RenewalRequest>
{
    Task<List<RenewalRequest>> GetByOldContractIdAsync(Guid oldContractId, CancellationToken ct = default);
    Task<RenewalRequest?> GetPendingByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task<bool> HasPendingForContractAsync(Guid contractId, CancellationToken ct = default);
}
