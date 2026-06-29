using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

public interface ILateFeeConfigService
{
    Task<List<LateFeeConfigDto>> GetListAsync(CancellationToken ct = default);
    Task<LateFeeConfigDto> GetActiveAsync(CancellationToken ct = default);
    Task<LateFeeConfigDto> SaveAsync(SaveLateFeeConfigRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
