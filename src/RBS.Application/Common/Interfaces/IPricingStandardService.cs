using RBS.Application.DTOs.Property;

namespace RBS.Application.Common.Interfaces;

public interface IPricingStandardService
{
    Task<List<PricingStandardDto>> GetListAsync(CancellationToken ct = default);
    Task<PricingStandardDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PricingStandardDto> CreateAsync(CreatePricingStandardRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdatePricingStandardRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
