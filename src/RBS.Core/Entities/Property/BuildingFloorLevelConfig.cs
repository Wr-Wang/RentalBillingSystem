namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

public class BuildingFloorLevelConfig : AssociationEntity, IHasLandlord
{
    public Guid BuildingId { get; private set; }
    public Guid FloorLevelBandId { get; private set; }
    public Guid LandlordId { get; private set; }
    private BuildingFloorLevelConfig() { }
    public BuildingFloorLevelConfig(Guid buildingId, Guid floorLevelBandId, Guid landlordId)
    { BuildingId = buildingId; FloorLevelBandId = floorLevelBandId; LandlordId = landlordId; }
}
