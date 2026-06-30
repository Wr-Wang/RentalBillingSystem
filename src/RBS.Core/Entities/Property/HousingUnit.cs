namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

public class HousingUnit : AggregateRoot, IHasCompany
{
    public string BuildingName { get; private set; } = string.Empty;
    public string? BuildingCode { get; private set; }
    public string? BuildingAddress { get; private set; }
    public Guid CompanyId { get; private set; }
    public string FloorName { get; private set; } = string.Empty;
    public int FloorSortOrder { get; private set; }
    public string UnitNo { get; private set; } = string.Empty;
    public string? FullCode { get; private set; }
    public Guid? RoomTypeId { get; private set; }
    public decimal? Area { get; private set; }
    public string? Orientation { get; private set; }
    public decimal? BaseRentAmount { get; private set; }
    public RoomStatus Status { get; private set; } = RoomStatus.Vacant;
    private HousingUnit() { }
    public HousingUnit(string buildingName, string floorName, int floorSortOrder, string unitNo, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(buildingName)) throw new ArgumentException("座楼名称不能为空");
        if (string.IsNullOrWhiteSpace(unitNo)) throw new ArgumentException("房源编号不能为空");
        BuildingName = buildingName.Trim(); FloorName = floorName.Trim();
        FloorSortOrder = floorSortOrder; UnitNo = unitNo.Trim(); CompanyId = companyId; Status = RoomStatus.Vacant;
    }
    public void SetBuildingInfo(string? code, string? address) { BuildingCode = code?.Trim(); BuildingAddress = address?.Trim(); }
    public void SetBuildingName(string name) { if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("座楼名称不能为空"); BuildingName = name.Trim(); }
    public void SetUnitNo(string unitNo) { if (string.IsNullOrWhiteSpace(unitNo)) throw new ArgumentException("房源编号不能为空"); UnitNo = unitNo.Trim(); }
    public void SetFloor(string name, int sortOrder) { FloorName = name.Trim(); FloorSortOrder = sortOrder; }
    public void SetFullCode(string? fullCode) => FullCode = fullCode?.Trim();
    public void SetRoomType(Guid? t) => RoomTypeId = t;
    public void SetArea(decimal? a) { if (a.HasValue && a <= 0) throw new ArgumentException("面积必须大于0"); Area = a; }
    public void SetOrientation(string? o) => Orientation = o?.Trim();
    public void SetBaseRentAmount(decimal? a) { if (a.HasValue && a <= 0) throw new ArgumentException("租金必须大于0"); BaseRentAmount = a; }
    public void UpdateDetails(Guid? rt, decimal? ar, string? or, decimal? br) { SetRoomType(rt); SetArea(ar); SetOrientation(or); SetBaseRentAmount(br); }
    public void Occupy() { if (Status != RoomStatus.Vacant) throw new InvalidOperationException($"房源状态为「{Status.DisplayName}」，无法出租"); Status = RoomStatus.Rented; }
    public void Vacate() { if (Status != RoomStatus.Rented) throw new InvalidOperationException($"房源状态为「{Status.DisplayName}」，无法退租"); Status = RoomStatus.Vacant; }
    public void SetMaintenance() { if (Status != RoomStatus.Vacant && Status != RoomStatus.Rented) throw new InvalidOperationException($"房源状态为「{Status.DisplayName}」，无法设为维修"); Status = RoomStatus.Maintenance; }
    public bool IsVacant => Status == RoomStatus.Vacant; public bool IsRented => Status == RoomStatus.Rented;
}
