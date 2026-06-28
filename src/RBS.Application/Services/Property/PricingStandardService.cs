using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Property;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Property;

public class PricingStandardService : IPricingStandardService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public PricingStandardService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }

    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<PricingStandardDto>> GetListAsync(CancellationToken ct = default)
    {
        var items = await _uow.RoomPricingStandards.GetAllAsync(ct);
        var dtos = new List<PricingStandardDto>();
        foreach (var item in items)
            dtos.Add(await MapToDtoAsync(item, ct));
        return dtos;
    }

    public async Task<PricingStandardDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _uow.RoomPricingStandards.GetByIdAsync(id, ct);
        return item == null ? null : await MapToDtoAsync(item, ct);
    }

    public async Task<PricingStandardDto> CreateAsync(CreatePricingStandardRequest request, CancellationToken ct = default)
    {
        var entity = new RoomPricingStandard(request.RoomTypeId, request.FloorLevelBandId, request.RentAmount, CompanyId);
        await _uow.RoomPricingStandards.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return await MapToDtoAsync(entity, ct);
    }

    public async Task UpdateAsync(Guid id, UpdatePricingStandardRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.RoomPricingStandards.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("定价标准不存在");
        if (request.RoomTypeId.HasValue) entity.SetRoomType(request.RoomTypeId.Value);
        if (request.FloorLevelBandId.HasValue) entity.SetFloorLevelBand(request.FloorLevelBandId.Value);
        if (request.RentAmount.HasValue) entity.SetRentAmount(request.RentAmount.Value);
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.RoomPricingStandards.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("定价标准不存在");
        await _uow.RoomPricingStandards.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private async Task<PricingStandardDto> MapToDtoAsync(RoomPricingStandard p, CancellationToken ct)
    {
        var dto = new PricingStandardDto
        {
            Id = p.Id, RoomTypeId = p.RoomTypeId,
            FloorLevelBandId = p.FloorLevelBandId, RentAmount = p.RentAmount
        };
        var rt = await _uow.RoomTypes.GetByIdAsync(p.RoomTypeId, ct);
        dto.RoomTypeName = rt?.Name;
        var fl = await _uow.FloorLevelBands.GetByIdAsync(p.FloorLevelBandId, ct);
        dto.FloorLevelBandName = fl?.Name;
        return dto;
    }
}
