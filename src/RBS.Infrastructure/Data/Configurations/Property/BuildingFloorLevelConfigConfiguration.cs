using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Property;

namespace RBS.Infrastructure.Data.Configurations.Property;

public class BuildingFloorLevelConfigConfiguration : IEntityTypeConfiguration<BuildingFloorLevelConfig>
{
    public void Configure(EntityTypeBuilder<BuildingFloorLevelConfig> builder)
    {
        builder.ToTable("BuildingFloorLevelConfigs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BuildingId).IsRequired().HasComment("楼宇ID");
        builder.Property(e => e.FloorLevelBandId).IsRequired().HasComment("楼层级别ID");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => new { e.BuildingId, e.FloorLevelBandId }).IsUnique();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
