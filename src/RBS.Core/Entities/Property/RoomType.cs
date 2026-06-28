namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

/// <summary>
/// 房型
/// </summary>
public class RoomType : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private RoomType() { }

    public RoomType(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("房型名称不能为空", nameof(name));
        Name = name;
        IsActive = true;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("房型名称不能为空", nameof(name));
        Name = name;
    }
    public void SetDescription(string? description) => Description = description;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
