namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

public class FloorLevelBand : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public int MinLevel { get; private set; }
    public int MaxLevel { get; private set; }
    public string? Description { get; private set; }
    private FloorLevelBand() { }
    public FloorLevelBand(string name, int minLevel, int maxLevel) { Name = name; MinLevel = minLevel; MaxLevel = maxLevel; }
}
