namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

/// <summary>
/// 房源聚合根 — 房屋单元，管理房源的基础信息和生命周期
/// 每个 HousingUnit 代表一个具体的可出租房间/铺位
/// 状态由 RoomStatus 枚举管理，仅允许 Vacant -> Rented -> Vacant 等有效流转
/// </summary>
public class HousingUnit : AggregateRoot, IHasCompany
{
    /// <summary>座楼名称，如 "A栋"、"B座" 等</summary>
    public string BuildingName { get; private set; } = string.Empty;
    /// <summary>座楼编号，可选，如 "A-001"</summary>
    public string? BuildingCode { get; private set; }
    /// <summary>座楼地址，可选</summary>
    public string? BuildingAddress { get; private set; }
    /// <summary>所属公司标识，用于多租户数据隔离</summary>
    public Guid CompanyId { get; private set; }
    /// <summary>楼层名称，如 "1层"、"2层"、"夹层"</summary>
    public string FloorName { get; private set; } = string.Empty;
    /// <summary>楼层排序序号，用于自然层排序（1为最底层）</summary>
    public int FloorSortOrder { get; private set; }
    /// <summary>房源编号/房间号，同一楼层内唯一，如 "101"、"A01"</summary>
    public string UnitNo { get; private set; } = string.Empty;
    /// <summary>房源全编码，由座楼+楼层+房源号组合而成，可选</summary>
    public string? FullCode { get; private set; }
    /// <summary>房型标识，指向 RoomType 字典表</summary>
    public Guid? RoomTypeId { get; private set; }
    /// <summary>建筑面积（平方米），必须大于 0</summary>
    public decimal? Area { get; private set; }
    /// <summary>朝向，如 "东"、"南"、"南北通透"</summary>
    public string? Orientation { get; private set; }
    /// <summary>基础租金金额（标准定价），实际合同租金以合同为准</summary>
    public decimal? BaseRentAmount { get; private set; }
    /// <summary>房源状态：Vacant（空置）/ Rented（已租）/ Maintenance（维修中）</summary>
    public RoomStatus Status { get; private set; } = RoomStatus.Vacant;

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private HousingUnit() { }

    /// <summary>
    /// 创建房源实例，初始状态为空置
    /// </summary>
    /// <param name="buildingName">座楼名称，不能为空</param>
    /// <param name="floorName">楼层名称</param>
    /// <param name="floorSortOrder">楼层排序序号</param>
    /// <param name="unitNo">房源编号/房间号，不能为空</param>
    /// <param name="companyId">所属公司标识</param>
    /// <exception cref="ArgumentException">当座楼名称或房源编号为空时抛出</exception>
    public HousingUnit(string buildingName, string floorName, int floorSortOrder, string unitNo, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(buildingName)) throw new ArgumentException("座楼名称不能为空");
        if (string.IsNullOrWhiteSpace(unitNo)) throw new ArgumentException("房源编号不能为空");
        BuildingName = buildingName.Trim(); FloorName = floorName.Trim();
        FloorSortOrder = floorSortOrder; UnitNo = unitNo.Trim(); CompanyId = companyId; Status = RoomStatus.Vacant;
    }

    /// <summary>设置座楼编号和地址</summary>
    /// <param name="code">座楼编号，null 表示清空</param>
    /// <param name="address">座楼地址，null 表示清空</param>
    public void SetBuildingInfo(string? code, string? address) { BuildingCode = code?.Trim(); BuildingAddress = address?.Trim(); }

    /// <summary>修改座楼名称</summary>
    /// <param name="name">新座楼名称，不能为空</param>
    /// <exception cref="ArgumentException">名称为空时抛出</exception>
    public void SetBuildingName(string name) { if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("座楼名称不能为空"); BuildingName = name.Trim(); }

    /// <summary>修改房源编号/房间号</summary>
    /// <param name="unitNo">新编号，不能为空</param>
    /// <exception cref="ArgumentException">编号为空时抛出</exception>
    public void SetUnitNo(string unitNo) { if (string.IsNullOrWhiteSpace(unitNo)) throw new ArgumentException("房源编号不能为空"); UnitNo = unitNo.Trim(); }

    /// <summary>设置楼层信息</summary>
    /// <param name="name">楼层名称</param>
    /// <param name="sortOrder">楼层排序序号</param>
    public void SetFloor(string name, int sortOrder) { FloorName = name.Trim(); FloorSortOrder = sortOrder; }

    /// <summary>设置房源全编码，null 表示清空</summary>
    public void SetFullCode(string? fullCode) => FullCode = fullCode?.Trim();

    /// <summary>设置房型</summary>
    /// <param name="t">房型标识，null 表示未指定</param>
    public void SetRoomType(Guid? t) => RoomTypeId = t;

    /// <summary>设置建筑面积</summary>
    /// <param name="a">面积值（平方米），必须大于 0，null 表示未设置</param>
    /// <exception cref="ArgumentException">面积小于等于 0 时抛出</exception>
    public void SetArea(decimal? a) { if (a.HasValue && a <= 0) throw new ArgumentException("面积必须大于0"); Area = a; }

    /// <summary>设置朝向，null 表示清空</summary>
    public void SetOrientation(string? o) => Orientation = o?.Trim();

    /// <summary>设置基础租金</summary>
    /// <param name="a">基础租金金额，必须大于 0，null 表示未设置</param>
    /// <exception cref="ArgumentException">租金小于等于 0 时抛出</exception>
    public void SetBaseRentAmount(decimal? a) { if (a.HasValue && a <= 0) throw new ArgumentException("租金必须大于0"); BaseRentAmount = a; }

    /// <summary>批量更新房源详情（房型、面积、朝向、基础租金）</summary>
    public void UpdateDetails(Guid? rt, decimal? ar, string? or, decimal? br) { SetRoomType(rt); SetArea(ar); SetOrientation(or); SetBaseRentAmount(br); }

    /// <summary>
    /// 出租房源，仅空置状态可出租
    /// </summary>
    /// <exception cref="InvalidOperationException">当房源不为空置状态时抛出</exception>
    public void Occupy() { if (Status != RoomStatus.Vacant) throw new InvalidOperationException($"房源状态为「{Status.DisplayName}」，无法出租"); Status = RoomStatus.Rented; }

    /// <summary>
    /// 退租房源，仅已租状态可退租
    /// </summary>
    /// <exception cref="InvalidOperationException">当房源不为已租状态时抛出</exception>
    public void Vacate() { if (Status != RoomStatus.Rented) throw new InvalidOperationException($"房源状态为「{Status.DisplayName}」，无法退租"); Status = RoomStatus.Vacant; }

    /// <summary>
    /// 将房源设为维修状态（空置或已租状态可转为维修）
    /// </summary>
    /// <exception cref="InvalidOperationException">当房源不允许转为维修状态时抛出</exception>
    public void SetMaintenance() { if (Status != RoomStatus.Vacant && Status != RoomStatus.Rented) throw new InvalidOperationException($"房源状态为「{Status.DisplayName}」，无法设为维修"); Status = RoomStatus.Maintenance; }

    /// <summary>是否空置</summary>
    public bool IsVacant => Status == RoomStatus.Vacant;
    /// <summary>是否已租</summary>
    public bool IsRented => Status == RoomStatus.Rented;
}
