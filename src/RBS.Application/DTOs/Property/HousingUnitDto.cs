namespace RBS.Application.DTOs.Property;
public class HousingUnitResponse
{
    public Guid Id { get; set; } public string BuildingName { get; set; } = "";
    public string? BuildingCode { get; set; } public string? BuildingAddress { get; set; }
    public string FloorName { get; set; } = ""; public int FloorSortOrder { get; set; }
    public string UnitNo { get; set; } = ""; public string? FullCode { get; set; }
    public Guid? RoomTypeId { get; set; } public string? RoomTypeName { get; set; }
    public string Status { get; set; } = "Vacant"; public decimal? Area { get; set; }
    public string? Orientation { get; set; } public decimal? BaseRentAmount { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HousingUnitStatsResponse
{
    public int Total { get; set; }
    public int Vacant { get; set; }
    public int Rented { get; set; }
    public int Maintenance { get; set; }
}
public class CreateHousingUnitRequest
{
    public string BuildingName { get; set; } = ""; public string? BuildingCode { get; set; }
    public string? BuildingAddress { get; set; } public string FloorName { get; set; } = "";
    public int FloorSortOrder { get; set; } public string UnitNo { get; set; } = "";
    public string? FullCode { get; set; } public Guid? RoomTypeId { get; set; }
    public decimal? Area { get; set; } public string? Orientation { get; set; }
    public decimal? BaseRentAmount { get; set; } public Guid CompanyId { get; set; }
}
public class UpdateHousingUnitRequest
{
    public string? BuildingName { get; set; }
    public string? BuildingCode { get; set; }
    public string? BuildingAddress { get; set; }
    public string? FloorName { get; set; }
    public int? FloorSortOrder { get; set; }
    public string? UnitNo { get; set; }
    public Guid? RoomTypeId { get; set; }
    public decimal? Area { get; set; }
    public string? Orientation { get; set; }
    public decimal? BaseRentAmount { get; set; }
    public string? Status { get; set; }
}
