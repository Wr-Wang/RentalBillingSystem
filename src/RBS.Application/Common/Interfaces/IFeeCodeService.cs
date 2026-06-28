using RBS.Application.DTOs.Billing;

namespace RBS.Application.Common.Interfaces;

public interface IFeeCodeService
{
    Task<List<FeeCodeDto>> GetListAsync(CancellationToken ct = default);
    Task<FeeCodeDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FeeCodeDto> CreateAsync(CreateFeeCodeRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateFeeCodeRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
