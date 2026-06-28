using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Property;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Property;

public class RoomTypeService : IRoomTypeService
{
    private readonly IUnitOfWork _uow;
    public RoomTypeService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<RoomTypeDto>> GetListAsync(CancellationToken ct = default)
    {
        var items = await _uow.RoomTypes.GetAllAsync(ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<RoomTypeDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.RoomTypes.GetByIdAsync(id, ct);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<RoomTypeDto> CreateAsync(CreateRoomTypeRequest request, CancellationToken ct = default)
    {
        var entity = new RoomType(request.Name);
        entity.SetDescription(request.Description);
        await _uow.RoomTypes.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateRoomTypeRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.RoomTypes.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("房型不存在");

        if (request.Name != null) entity.Rename(request.Name);
        if (request.Description != null) entity.SetDescription(request.Description);
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) entity.Activate();
            else entity.Deactivate();
        }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.RoomTypes.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("房型不存在");
        await _uow.RoomTypes.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private static RoomTypeDto MapToDto(RoomType r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        IsActive = r.IsActive
    };
}
