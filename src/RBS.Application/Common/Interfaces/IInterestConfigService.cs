using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

public interface IInterestConfigService
{
    Task<List<InterestConfigDto>> GetListAsync(CancellationToken ct = default);
    Task<InterestConfigDto> GetActiveAsync(CancellationToken ct = default);
    Task<InterestConfigDto> SaveAsync(SaveInterestConfigRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
