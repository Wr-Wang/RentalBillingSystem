using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>房源导入行 — TPH 继承自 ImportBatchItem，ImportType='HousingUnit' 时有值</summary>
public class ImportBatchItemHousingUnit : ImportBatchItem
{
    // ===== 座楼信息 =====
    public string? BuildingName { get; private set; }
    public string? BuildingCode { get; private set; }
    public string? BuildingAddress { get; private set; }

    // ===== 楼层房号 =====
    public string? FloorName { get; private set; }
    public int? FloorSortOrder { get; private set; }
    public string? UnitNo { get; private set; }
    public string? FullCode { get; private set; }

    // ===== 房屋属性 =====
    public Guid? RoomTypeId { get; private set; }
    public string? RoomTypeName { get; private set; }
    public decimal? Area { get; private set; }
    public string? Orientation { get; private set; }

    // ===== 预计算 =====
    public decimal? BaseRentAmount { get; private set; }
    public string? PriceWarning { get; private set; }

    private ImportBatchItemHousingUnit() : base() { }

    public ImportBatchItemHousingUnit(Guid importBatchId, int rowIndex) : base(importBatchId, "HousingUnit", rowIndex) { }

    public void SetBuildingInfo(string? buildingName, string? buildingCode, string? buildingAddress)
    {
        BuildingName = buildingName;
        BuildingCode = buildingCode;
        BuildingAddress = buildingAddress;
    }

    public void SetRoomInfo(string? floorName, int? floorSortOrder, string? unitNo, string? fullCode)
    {
        FloorName = floorName;
        FloorSortOrder = floorSortOrder;
        UnitNo = unitNo;
        FullCode = fullCode;
    }

    public void SetPropertyInfo(Guid? roomTypeId, string? roomTypeName, decimal? area, string? orientation)
    {
        RoomTypeId = roomTypeId;
        RoomTypeName = roomTypeName;
        Area = area;
        Orientation = orientation;
    }

    public void SetPricingInfo(decimal? baseRentAmount, string? priceWarning)
    {
        BaseRentAmount = baseRentAmount;
        PriceWarning = priceWarning;
    }

    public void Clear()
    {
        SetBuildingInfo(null, null, null);
        SetRoomInfo(null, null, null, null);
        SetPropertyInfo(null, null, null, null);
        SetPricingInfo(null, null);
    }
}
