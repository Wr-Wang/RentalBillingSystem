using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 公司管理应用服务
/// </summary>
public interface ICompanyService
{
    Task<PagedResult<CompanyDto>> GetPagedAsync(CompanyQuery query, CancellationToken ct = default);
    Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CreateCompanyRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CompanyStatsDto?> GetStatsAsync(Guid id, CancellationToken ct = default);
}
