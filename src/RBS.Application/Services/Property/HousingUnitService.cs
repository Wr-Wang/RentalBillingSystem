using RBS.Application.Common.Interfaces; using RBS.Application.DTOs.Property;
using RBS.Core.Entities.Property; using RBS.Core.Interfaces.Services; using RBS.Core.Interfaces.UnitOfWork;
namespace RBS.Application.Services.Property;
public class HousingUnitService : IHousingUnitService
{
    private readonly IUnitOfWork _uow; private readonly ITenantService _ts;
    public HousingUnitService(IUnitOfWork uow, ITenantService ts) { _uow = uow; _ts = ts; }

    public async Task<List<HousingUnitResponse>> GetListAsync(string? bn = null, string? kw = null, string? st = null, CancellationToken ct = default)
    {
        var all = await _uow.HousingUnits.GetAllAsync(ct);
        var cid = _ts.EffectiveCompanyId;
        var q = all.Where(u => !cid.HasValue || u.CompanyId == cid.Value);
        if (!string.IsNullOrEmpty(bn)) q = q.Where(u => u.BuildingName == bn);
        if (!string.IsNullOrEmpty(st)) q = q.Where(u => u.Status.Code == st);
        if (!string.IsNullOrEmpty(kw)) q = q.Where(u => u.UnitNo.Contains(kw) || (u.FullCode?.Contains(kw) ?? false) || (u.BuildingAddress?.Contains(kw) ?? false) || u.BuildingName.Contains(kw));
        return await MapAsync(q.OrderBy(u => u.BuildingName).ThenBy(u => u.FloorSortOrder).ThenBy(u => u.UnitNo).ToList(), ct);
    }

    public async Task<HousingUnitStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        var all = await _uow.HousingUnits.GetAllAsync(ct);
        var cid = _ts.EffectiveCompanyId;
        var q = all.Where(u => !cid.HasValue || u.CompanyId == cid.Value).ToList();
        return new HousingUnitStatsResponse
        {
            Total = q.Count,
            Vacant = q.Count(u => u.Status.Code == "Vacant"),
            Rented = q.Count(u => u.Status.Code == "Rented"),
            Maintenance = q.Count(u => u.Status.Code == "Maintenance")
        };
    }
    public async Task<HousingUnitResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _uow.HousingUnits.GetByIdAsync(id, ct);
        return e == null ? null : (await MapAsync(new[] { e }, ct)).FirstOrDefault();
    }
    public async Task<HousingUnitResponse> CreateAsync(CreateHousingUnitRequest r, CancellationToken ct = default)
    {
        var u = new HousingUnit(r.BuildingName, r.FloorName, r.FloorSortOrder, r.UnitNo, r.CompanyId);
        u.SetBuildingInfo(r.BuildingCode, r.BuildingAddress); u.SetFullCode(r.FullCode);
        u.UpdateDetails(r.RoomTypeId, r.Area, r.Orientation, r.BaseRentAmount);
        await _uow.HousingUnits.AddAsync(u, ct); await _uow.CommitAsync(ct);
        return (await MapAsync(new[] { u }, ct)).First()!;
    }
    public async Task UpdateAsync(Guid id, UpdateHousingUnitRequest r, CancellationToken ct = default)
    {
        var u = await _uow.HousingUnits.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("房源不存在");
        if (!string.IsNullOrEmpty(r.BuildingName)) u.SetBuildingName(r.BuildingName);
        if (r.BuildingCode != null || r.BuildingAddress != null) u.SetBuildingInfo(r.BuildingCode ?? u.BuildingCode, r.BuildingAddress ?? u.BuildingAddress);
        if (!string.IsNullOrEmpty(r.UnitNo)) u.SetUnitNo(r.UnitNo);
        if (r.FloorName != null || r.FloorSortOrder.HasValue) u.SetFloor(r.FloorName ?? u.FloorName, r.FloorSortOrder ?? u.FloorSortOrder);
        if (r.RoomTypeId.HasValue || r.Area.HasValue || r.Orientation != null || r.BaseRentAmount.HasValue)
            u.UpdateDetails(r.RoomTypeId, r.Area, r.Orientation, r.BaseRentAmount);
        if (!string.IsNullOrEmpty(r.Status)) { switch (r.Status) { case "Rented": u.Occupy(); break; case "Vacant": u.Vacate(); break; case "Maintenance": u.SetMaintenance(); break; } }
        await _uow.CommitAsync(ct);
    }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var u = await _uow.HousingUnits.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("房源不存在");
        await _uow.HousingUnits.DeleteAsync(u, ct); await _uow.CommitAsync(ct);
    }
    public async Task<List<TreeBuildingDto>> GetTreeAsync(Guid companyId, CancellationToken ct = default)
    {
        var all = await _uow.HousingUnits.GetAllAsync(ct);
        var units = (companyId == Guid.Empty ? all : all.Where(u => u.CompanyId == companyId))
            .OrderBy(u => u.BuildingName).ThenBy(u => u.FloorSortOrder).ThenBy(u => u.UnitNo).ToList();
        return units.GroupBy(u => u.BuildingName).Select(g => new TreeBuildingDto { Id = "B:" + g.Key, Name = g.Key,
            Children = g.GroupBy(u => u.FloorName).Select(fg => new TreeFloorDto { Id = "F:" + fg.Key, Name = fg.Key,
                Children = fg.Select(u => new TreeRoomDto { Id = u.Id.ToString(), Name = u.FullCode ?? u.UnitNo }).ToList() }).ToList() }).ToList();
    }
    private async Task<List<HousingUnitResponse>> MapAsync(IEnumerable<HousingUnit> units, CancellationToken ct)
    {
        var tIds = units.Where(u => u.RoomTypeId.HasValue).Select(u => u.RoomTypeId!.Value).Distinct().ToList();
        var types = tIds.Count > 0 ? (await _uow.RoomTypes.GetAllAsync(ct)).Where(t => tIds.Contains(t.Id)).ToDictionary(t => t.Id, t => t.Name) : new();
        return units.Select(u => new HousingUnitResponse
        {
            Id = u.Id, BuildingName = u.BuildingName, BuildingCode = u.BuildingCode, BuildingAddress = u.BuildingAddress,
            FloorName = u.FloorName, FloorSortOrder = u.FloorSortOrder, UnitNo = u.UnitNo, FullCode = u.FullCode,
            RoomTypeId = u.RoomTypeId, RoomTypeName = u.RoomTypeId.HasValue ? types.GetValueOrDefault(u.RoomTypeId.Value) : null,
            Status = u.Status.Code, Area = u.Area, Orientation = u.Orientation, BaseRentAmount = u.BaseRentAmount, CompanyId = u.CompanyId, CreatedAt = u.CreatedAt
        }).ToList();
    }
}
