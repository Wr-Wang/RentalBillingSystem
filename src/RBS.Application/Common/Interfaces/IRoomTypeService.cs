using RBS.Application.DTOs.Property;

namespace RBS.Application.Common.Interfaces;

public interface IRoomTypeService
{
    Task<List<RoomTypeDto>> GetListAsync(CancellationToken ct = default);
    Task<RoomTypeDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RoomTypeDto> CreateAsync(CreateRoomTypeRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateRoomTypeRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
