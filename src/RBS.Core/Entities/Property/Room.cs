namespace RBS.Core.Entities.Property;

using RBS.Core.Entities.Base;

/// <summary>
/// 房间实体（聚合根）
/// 表示建筑物中一个独立的房间或单元，被合同独立引用
/// </summary>
public class Room : AggregateRoot
{
    /// <summary>
    /// 所属建筑唯一标识
    /// </summary>
    public Guid BuildingId { get; private set; }

    /// <summary>
    /// 所属楼层唯一标识
    /// </summary>
    public Guid FloorId { get; private set; }

    /// <summary>
    /// 房间编号（如 "101"、"A001" 等）
    /// </summary>
    public string RoomNo { get; private set; } = string.Empty;

    /// <summary>
    /// 房间完整编码（如 "A栋-1层-101"）
    /// </summary>
    public string? FullCode { get; private set; }

    /// <summary>
    /// 房间类型标识，指向 RoomType 字典表
    /// </summary>
    public Guid? RoomTypeId { get; private set; }

    /// <summary>
    /// 房间面积（平方米）
    /// </summary>
    public decimal? Area { get; private set; }

    /// <summary>
    /// 房间当前状态（空置/已租/维修中）
    /// </summary>
    public RoomStatus Status { get; private set; } = RoomStatus.Vacant;

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private Room() { }

    /// <summary>
    /// 创建房间实例
    /// </summary>
    /// <param name="buildingId">所属建筑标识</param>
    /// <param name="floorId">所属楼层标识</param>
    /// <param name="roomNo">房间编号</param>
    /// <exception cref="ArgumentException">当房间编号为空或仅空白时抛出</exception>
    public Room(Guid buildingId, Guid floorId, string roomNo)
    {
        if (string.IsNullOrWhiteSpace(roomNo))
            throw new ArgumentException("房间编号不能为空", nameof(roomNo));

        BuildingId = buildingId;
        FloorId = floorId;
        RoomNo = roomNo.Trim();
        Status = RoomStatus.Vacant;
    }

    /// <summary>
    /// 设置房间类型
    /// </summary>
    /// <param name="roomTypeId">房间类型标识</param>
    public void SetRoomType(Guid? roomTypeId)
    {
        RoomTypeId = roomTypeId;
    }

    /// <summary>
    /// 设置房间面积
    /// </summary>
    /// <param name="area">房间面积（平方米），必须大于 0</param>
    /// <exception cref="ArgumentException">当面积不大于 0 时抛出</exception>
    public void SetArea(decimal? area)
    {
        if (area.HasValue && area.Value <= 0)
            throw new ArgumentException("房间面积必须大于 0", nameof(area));

        Area = area;
    }

    /// <summary>
    /// 设置房间完整编码
    /// </summary>
    /// <param name="fullCode">完整编码（允许 null 表示未设置）</param>
    public void SetFullCode(string? fullCode)
    {
        FullCode = fullCode?.Trim();
    }

    /// <summary>
    /// 将房间标记为已租状态
    /// </summary>
    /// <exception cref="InvalidOperationException">当房间当前状态不允许租出时抛出</exception>
    public void Occupy()
    {
        if (Status != RoomStatus.Vacant)
            throw new InvalidOperationException($"房间当前状态为「{Status.DisplayName}」，无法执行出租操作，仅空置房间可被出租");

        Status = RoomStatus.Rented;
    }

    /// <summary>
    /// 将房间标记为空置状态
    /// </summary>
    /// <exception cref="InvalidOperationException">当房间当前状态不允许退租时抛出</exception>
    public void Vacate()
    {
        if (Status != RoomStatus.Rented)
            throw new InvalidOperationException($"房间当前状态为「{Status.DisplayName}」，无法执行退租操作，仅已租房间可退租");

        Status = RoomStatus.Vacant;
    }

    /// <summary>
    /// 将房间标记为维修中状态
    /// </summary>
    /// <exception cref="InvalidOperationException">当房间当前状态不允许转为维修时抛出</exception>
    public void SetMaintenance()
    {
        if (Status != RoomStatus.Vacant && Status != RoomStatus.Rented)
            throw new InvalidOperationException($"房间当前状态为「{Status.DisplayName}」，无法设置为维修中");

        Status = RoomStatus.Maintenance;
    }

    /// <summary>
    /// 房间是否为空置状态
    /// </summary>
    public bool IsVacant => Status == RoomStatus.Vacant;

    /// <summary>
    /// 房间是否为已租状态
    /// </summary>
    public bool IsRented => Status == RoomStatus.Rented;
}
