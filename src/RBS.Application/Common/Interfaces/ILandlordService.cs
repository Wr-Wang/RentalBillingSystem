using RBS.Application.DTOs.Organization;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 房东管理应用服务
/// </summary>
public interface ILandlordService
{
    Task<PagedResult<LandlordDto>> GetPagedAsync(LandlordQuery query, CancellationToken ct = default);
    Task<LandlordDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LandlordDto> CreateAsync(CreateLandlordRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CreateLandlordRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<LandlordStatsDto?> GetStatsAsync(Guid id, CancellationToken ct = default);
}
