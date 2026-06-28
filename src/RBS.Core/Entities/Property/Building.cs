namespace RBS.Core.Entities.Property;

using RBS.Core.Entities.Base;

/// <summary>
/// 楼宇聚合根 — 房产的顶层组织单位
/// </summary>
public class Building : AggregateRoot, IHasCompany
{
    public string Name { get; private set; }
    public string? Code { get; private set; }
    public string? Address { get; private set; }
    public Guid CompanyId { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Floor> _floors = new();
    public IReadOnlyCollection<Floor> Floors => _floors.AsReadOnly();

    private Building() : base()
    {
        Name = string.Empty;
    }

    /// <summary>领域构造函数</summary>
    public Building(string name, Guid companyId) : base()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("楼宇名称不能为空", nameof(name));
        Name = name;
        CompanyId = companyId;
        IsActive = true;
    }

    // ===== 属性设置 =====
    public void SetCode(string code) => Code = code;
    public void SetAddress(string address) => Address = address;
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("名称不能为空");
        Name = newName;
    }
    public void Activate() => IsActive = true;
    public void Deactivate()
    {
        if (_floors.Any(f => f.Rooms.Any(r => r.IsRented)))
            throw new InvalidOperationException("楼宇中存在已租房间，不能停用");
        IsActive = false;
    }

    // ===== 楼层管理 =====
    public Floor AddFloor(string name, int sortOrder = 0)
    {
        if (_floors.Any(f => f.Name == name))
            throw new InvalidOperationException($"楼层 '{name}' 已存在");
        var floor = new Floor(Id, name) { SortOrder = sortOrder };
        _floors.Add(floor);
        return floor;
    }

    public void RemoveFloor(Guid floorId)
    {
        var floor = _floors.FirstOrDefault(f => f.Id == floorId)
            ?? throw new InvalidOperationException("未找到该楼层");
        if (floor.Rooms.Any())
            throw new InvalidOperationException("楼层中存在房间，不能删除");
        _floors.Remove(floor);
    }
}
