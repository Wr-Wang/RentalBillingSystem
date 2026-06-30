using RBS.Application.DTOs.Property;
namespace RBS.Application.Common.Interfaces;
public interface IHousingUnitService
{
    Task<List<HousingUnitResponse>> GetListAsync(string? buildingName = null, string? keyword = null, string? status = null, CancellationToken ct = default);
    Task<HousingUnitResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HousingUnitResponse> CreateAsync(CreateHousingUnitRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateHousingUnitRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<TreeBuildingDto>> GetTreeAsync(Guid companyId, CancellationToken ct = default);
    Task<HousingUnitStatsResponse> GetStatsAsync(CancellationToken ct = default);
}
