namespace RBS.Application.DTOs.Property;

public class BuildingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public Guid LandlordId { get; set; }
    public bool IsActive { get; set; }
    public int FloorCount { get; set; }
    public int RoomCount { get; set; }
    public List<FloorDto>? Floors { get; set; }
}

public class FloorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<RoomDto>? Rooms { get; set; }
}

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNo { get; set; } = string.Empty;
    public string? FullCode { get; set; }
    public Guid? RoomTypeId { get; set; }
    public string Status { get; set; } = "Vacant";
    public decimal? Area { get; set; }
}

public class CreateBuildingRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public Guid LandlordId { get; set; }
}
