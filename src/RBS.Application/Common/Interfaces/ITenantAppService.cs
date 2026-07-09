using RBS.Application.DTOs.Contract;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

public interface ITenantAppService
{
    Task<PagedResult<TenantDto>> GetPagedAsync(Guid companyId, string? keyword, int page, int pageSize, CancellationToken ct = default);
    Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TenantDto> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
    Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsPhoneUniqueAsync(Guid companyId, string phone, Guid? excludeId, CancellationToken ct = default);
    Task<bool> IsIdCardUniqueAsync(Guid companyId, string idCard, Guid? excludeId, CancellationToken ct = default);
}
