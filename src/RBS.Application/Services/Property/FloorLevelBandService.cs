using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Property;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Property;

public class FloorLevelBandService : IFloorLevelBandService
{
    private readonly IUnitOfWork _uow;
    public FloorLevelBandService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<FloorLevelBandDto>> GetListAsync(CancellationToken ct = default)
        => (await _uow.FloorLevelBands.GetAllAsync(ct)).Select(MapToDto).ToList();

    public async Task<FloorLevelBandDto> CreateAsync(CreateFloorLevelBandRequest request, CancellationToken ct = default)
    {
        var entity = new FloorLevelBand(request.Name, request.MinLevel, request.MaxLevel);
        await _uow.FloorLevelBands.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateFloorLevelBandRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.FloorLevelBands.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("楼层级别不存在");
        if (request.Name != null) entity.Rename(request.Name);
        if (request.MinLevel.HasValue) entity.SetMinLevel(request.MinLevel.Value);
        if (request.MaxLevel.HasValue) entity.SetMaxLevel(request.MaxLevel.Value);
        if (request.Description != null) entity.SetDescription(request.Description);
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.FloorLevelBands.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("楼层级别不存在");
        await _uow.FloorLevelBands.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private static FloorLevelBandDto MapToDto(FloorLevelBand f) => new()
    { Id = f.Id, Name = f.Name, MinLevel = f.MinLevel, MaxLevel = f.MaxLevel, Description = f.Description };
}
