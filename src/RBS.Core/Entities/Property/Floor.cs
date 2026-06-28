namespace RBS.Core.Entities.Property;

using RBS.Core.Entities.Base;

/// <summary>
/// 楼层实体，表示建筑物中的一个楼层
/// 每个楼层包含名称、排序顺序以及所属房间集合
/// </summary>
public class Floor : AuditableEntity
{
    private readonly List<Room> _rooms = new();

    /// <summary>
    /// 所属建筑唯一标识
    /// </summary>
    public Guid BuildingId { get; private set; }

    /// <summary>
    /// 楼层名称（如 "1层"、"2层"、"夹层" 等）
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 排序顺序，用于按楼层自然顺序排列
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 楼层包含的房间集合（只读，外部不可直接修改）
    /// </summary>
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private Floor() { }

    /// <summary>
    /// 创建楼层实例
    /// </summary>
    /// <param name="buildingId">所属建筑标识</param>
    /// <param name="name">楼层名称</param>
    /// <exception cref="ArgumentException">当名称为空或仅空白时抛出</exception>
    public Floor(Guid buildingId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("楼层名称不能为空", nameof(name));

        BuildingId = buildingId;
        Name = name.Trim();
    }

    /// <summary>
    /// 重命名楼层
    /// </summary>
    /// <param name="newName">新的楼层名称</param>
    /// <exception cref="ArgumentException">当名称为空或仅空白时抛出</exception>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("楼层名称不能为空", nameof(newName));

        Name = newName.Trim();
    }

    /// <summary>
    /// 添加房间到本楼层
    /// </summary>
    /// <param name="room">需要添加的房间实体</param>
    /// <exception cref="ArgumentNullException">当房间参数为空时抛出</exception>
    internal void AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        _rooms.Add(room);
    }

    /// <summary>
    /// 从本楼层移除指定房间
    /// </summary>
    /// <param name="room">需要移除的房间实体</param>
    /// <exception cref="ArgumentNullException">当房间参数为空时抛出</exception>
    internal void RemoveRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        _rooms.Remove(room);
    }
}
