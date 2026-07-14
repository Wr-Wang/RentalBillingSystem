using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Import;

/// <summary>
/// 房源导入行 — TPH 继承自 ImportBatchItem
/// 当 ImportType='HousingUnit' 时 EF Core 使用该子类映射，
/// 包含房源导入所需的座楼信息、楼层房号、房屋属性及预计算的租金价格
/// </summary>
public class ImportBatchItemHousingUnit : ImportBatchItem
{
    // ===== 座楼信息 =====

    /// <summary>座楼/楼栋名称，如 "A座"、"1号楼"</summary>
    public string? BuildingName { get; private set; }

    /// <summary>座楼/楼栋编码，系统的楼栋短码标识</summary>
    public string? BuildingCode { get; private set; }

    /// <summary>座楼/楼栋地址，楼栋的物理位置描述</summary>
    public string? BuildingAddress { get; private set; }

    // ===== 楼层房号 =====

    /// <summary>楼层名称，如 "1层"、"2层"</summary>
    public string? FloorName { get; private set; }

    /// <summary>楼层排序序号，用于楼栋内楼层的排序</summary>
    public int? FloorSortOrder { get; private set; }

    /// <summary>房间号/单元号，如 "101"、"A01"</summary>
    public string? UnitNo { get; private set; }

    /// <summary>完整编码，由楼栋+楼层+房号组合生成的全局唯一编码</summary>
    public string? FullCode { get; private set; }

    // ===== 房屋属性 =====

    /// <summary>房型标识（可选），关联到房型字典数据</summary>
    public Guid? RoomTypeId { get; private set; }

    /// <summary>房型名称（可选），如 "一室一厅"、"开间"</summary>
    public string? RoomTypeName { get; private set; }

    /// <summary>房屋面积（平方米，可选）</summary>
    public decimal? Area { get; private set; }

    /// <summary>朝向（可选），如 "南"、"北"、"南北通透"</summary>
    public string? Orientation { get; private set; }

    // ===== 预计算 =====

    /// <summary>基础租金预估值（可选），由导入引擎根据规则自动计算</summary>
    public decimal? BaseRentAmount { get; private set; }

    /// <summary>价格异常警告（可选），如 "租金低于市场价30%" 等提示信息</summary>
    public string? PriceWarning { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ImportBatchItemHousingUnit() : base() { }

    /// <summary>
    /// 创建房源导入行实例。自动设置 ImportType 为 "HousingUnit"
    /// </summary>
    /// <param name="importBatchId">所属导入批次标识</param>
    /// <param name="rowIndex">数据行号</param>
    public ImportBatchItemHousingUnit(Guid importBatchId, int rowIndex) : base(importBatchId, "HousingUnit", rowIndex) { }

    /// <summary>设置座楼信息</summary>
    /// <param name="buildingName">座楼名称</param>
    /// <param name="buildingCode">座楼编码</param>
    /// <param name="buildingAddress">座楼地址</param>
    public void SetBuildingInfo(string? buildingName, string? buildingCode, string? buildingAddress)
    {
        BuildingName = buildingName;
        BuildingCode = buildingCode;
        BuildingAddress = buildingAddress;
    }

    /// <summary>设置楼层房号信息</summary>
    /// <param name="floorName">楼层名称</param>
    /// <param name="floorSortOrder">楼层排序序号</param>
    /// <param name="unitNo">房间号/单元号</param>
    /// <param name="fullCode">完整编码</param>
    public void SetRoomInfo(string? floorName, int? floorSortOrder, string? unitNo, string? fullCode)
    {
        FloorName = floorName;
        FloorSortOrder = floorSortOrder;
        UnitNo = unitNo;
        FullCode = fullCode;
    }

    /// <summary>设置房屋属性信息</summary>
    /// <param name="roomTypeId">房型标识</param>
    /// <param name="roomTypeName">房型名称</param>
    /// <param name="area">房屋面积（平方米）</param>
    /// <param name="orientation">朝向</param>
    public void SetPropertyInfo(Guid? roomTypeId, string? roomTypeName, decimal? area, string? orientation)
    {
        RoomTypeId = roomTypeId;
        RoomTypeName = roomTypeName;
        Area = area;
        Orientation = orientation;
    }

    /// <summary>设置租金预计算信息</summary>
    /// <param name="baseRentAmount">基础租金预估值</param>
    /// <param name="priceWarning">价格异常警告</param>
    public void SetPricingInfo(decimal? baseRentAmount, string? priceWarning)
    {
        BaseRentAmount = baseRentAmount;
        PriceWarning = priceWarning;
    }

    /// <summary>清空所有已设置的导入数据（全部置为 null），用于重置行数据</summary>
    public void Clear()
    {
        SetBuildingInfo(null, null, null);
        SetRoomInfo(null, null, null, null);
        SetPropertyInfo(null, null, null, null);
        SetPricingInfo(null, null);
    }
}
