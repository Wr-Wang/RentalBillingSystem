namespace RBS.Application.DTOs.Property;

public class PricingStandardDto
{
    public Guid Id { get; set; }
    public Guid RoomTypeId { get; set; }
    public string? RoomTypeName { get; set; }
    public Guid FloorLevelBandId { get; set; }
    public string? FloorLevelBandName { get; set; }
    public decimal RentAmount { get; set; }
}

public class CreatePricingStandardRequest
{
    public Guid RoomTypeId { get; set; }
    public Guid FloorLevelBandId { get; set; }
    public decimal RentAmount { get; set; }
}

public class UpdatePricingStandardRequest
{
    public Guid? RoomTypeId { get; set; }
    public Guid? FloorLevelBandId { get; set; }
    public decimal? RentAmount { get; set; }
}
