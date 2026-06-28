using RBS.Application.DTOs.Property;

namespace RBS.Application.Common.Interfaces;

public interface IFloorLevelBandService
{
    Task<List<FloorLevelBandDto>> GetListAsync(CancellationToken ct = default);
    Task<FloorLevelBandDto> CreateAsync(CreateFloorLevelBandRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateFloorLevelBandRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
