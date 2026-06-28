using RBS.Application.DTOs.SystemConfig;

namespace RBS.Application.Common.Interfaces;

public interface ITaxRateConfigService
{
    Task<List<TaxRateConfigDto>> GetListAsync(CancellationToken ct = default);
    Task<TaxRateConfigDto> CreateAsync(CreateTaxRateConfigRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateTaxRateConfigRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
