namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

public class BuildingFloorLevelConfig : AssociationEntity, IHasCompany
{
    public Guid BuildingId { get; private set; }
    public Guid FloorLevelBandId { get; private set; }
    public Guid CompanyId { get; private set; }
    private BuildingFloorLevelConfig() { }
    public BuildingFloorLevelConfig(Guid buildingId, Guid floorLevelBandId, Guid companyId)
    { BuildingId = buildingId; FloorLevelBandId = floorLevelBandId; CompanyId = companyId; }
}
