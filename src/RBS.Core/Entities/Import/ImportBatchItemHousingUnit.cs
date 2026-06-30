using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>房源导入行 — TPH 继承自 ImportBatchItem，ImportType='HousingUnit' 时有值</summary>
public class ImportBatchItemHousingUnit : ImportBatchItem
{
    // ===== 座楼信息 =====
    public string? BuildingName { get; set; }
    public string? BuildingCode { get; set; }
    public string? BuildingAddress { get; set; }

    // ===== 楼层房号 =====
    public string? FloorName { get; set; }
    public int? FloorSortOrder { get; set; }
    public string? UnitNo { get; set; }
    public string? FullCode { get; set; }

    // ===== 房屋属性 =====
    public Guid? RoomTypeId { get; set; }
    public string? RoomTypeName { get; set; }
    public decimal? Area { get; set; }
    public string? Orientation { get; set; }

    // ===== 预计算 =====
    public decimal? BaseRentAmount { get; set; }
    public string? PriceWarning { get; set; }

    private ImportBatchItemHousingUnit() : base() { }

    public ImportBatchItemHousingUnit(Guid importBatchId, int rowIndex) : base(importBatchId, "HousingUnit", rowIndex) { }
}
