namespace RBS.Application.DTOs.Property;

public class FloorLevelBandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public string? Description { get; set; }
}

public class CreateFloorLevelBandRequest
{
    public string Name { get; set; } = string.Empty;
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public string? Description { get; set; }
}

public class UpdateFloorLevelBandRequest
{
    public string? Name { get; set; }
    public int? MinLevel { get; set; }
    public int? MaxLevel { get; set; }
    public string? Description { get; set; }
}
