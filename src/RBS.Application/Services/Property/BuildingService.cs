using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Property;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Property;

/// <summary>
/// 楼宇管理应用服务
/// </summary>
public class BuildingService : IBuildingService
{
    private readonly IUnitOfWork _uow;

    public BuildingService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<BuildingDto>> GetListAsync(Guid landlordId, CancellationToken ct = default)
    {
        var buildings = await _uow.Buildings.GetByLandlordIdAsync(landlordId, ct);
        return buildings.Select(b => new BuildingDto
        {
            Id = b.Id,
            Name = b.Name,
            Code = b.Code,
            Address = b.Address,
            LandlordId = b.LandlordId,
            IsActive = b.IsActive,
            FloorCount = b.Floors.Count,
            RoomCount = b.Floors.Sum(f => f.Rooms.Count)
        }).ToList();
    }

    public async Task<BuildingDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var building = await _uow.Buildings.GetByIdAsync(id, ct);
        if (building == null) return null;
        return new BuildingDto
        {
            Id = building.Id,
            Name = building.Name,
            Code = building.Code,
            Address = building.Address,
            LandlordId = building.LandlordId,
            IsActive = building.IsActive,
            Floors = building.Floors.Select(f => new FloorDto
            {
                Id = f.Id,
                Name = f.Name,
                SortOrder = f.SortOrder,
                Rooms = f.Rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNo = r.RoomNo,
                    FullCode = r.FullCode,
                    RoomTypeId = r.RoomTypeId,
                    Status = r.Status.Code,
                    Area = r.Area
                }).ToList()
            }).ToList()
        };
    }

    public async Task<BuildingDto> CreateAsync(CreateBuildingRequest request, CancellationToken ct = default)
    {
        var building = new Building(request.Name, request.LandlordId);
        if (!string.IsNullOrEmpty(request.Code)) building.SetCode(request.Code);
        if (!string.IsNullOrEmpty(request.Address)) building.SetAddress(request.Address);

        await _uow.Buildings.AddAsync(building, ct);
        await _uow.CommitAsync(ct);
        return await GetByIdAsync(building.Id, ct) ?? throw new InvalidOperationException("创建失败");
    }

    public async Task UpdateAsync(Guid id, CreateBuildingRequest request, CancellationToken ct = default)
    {
        var building = await _uow.Buildings.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("楼宇不存在");
        building.Rename(request.Name);
        building.SetCode(request.Code ?? string.Empty);
        building.SetAddress(request.Address ?? string.Empty);
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var building = await _uow.Buildings.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("楼宇不存在");
        building.Deactivate();
        await _uow.CommitAsync(ct);
    }
}
