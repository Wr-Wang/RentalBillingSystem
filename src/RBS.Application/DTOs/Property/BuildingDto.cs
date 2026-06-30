namespace RBS.Application.DTOs.Property;

public class BuildingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; }
    public int FloorCount { get; set; }
    public int RoomCount { get; set; }
}

public class CreateBuildingRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public Guid CompanyId { get; set; }
    public int FloorCount { get; set; } = 1;
}

/// <summary>
/// 房屋结构树形 DTO（用于前端 el-tree）
/// </summary>
public class TreeBuildingDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<TreeFloorDto> Children { get; set; } = new();
}

public class TreeFloorDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<TreeRoomDto> Children { get; set; } = new();
}

public class TreeRoomDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
