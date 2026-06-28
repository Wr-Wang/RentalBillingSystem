using RBS.Application.DTOs.Property;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 楼宇管理应用服务
/// </summary>
public interface IBuildingService
{
    Task<List<BuildingDto>> GetListAsync(Guid landlordId, CancellationToken ct = default);
    Task<BuildingDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BuildingDto> CreateAsync(CreateBuildingRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CreateBuildingRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
