namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

public class RoomType : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    private RoomType() { }
    public RoomType(string name) { Name = name; }
}
