using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Property;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Property;

public class FloorLevelBandConfiguration : IEntityTypeConfiguration<FloorLevelBand>
{
    public void Configure(EntityTypeBuilder<FloorLevelBand> builder)
    {
        builder.ToTable("FloorLevelBands");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(50).HasComment("楼层级别名称（低区/中区/高区）");
        builder.Property(e => e.Description).HasMaxLength(200).HasComment("楼层级别描述");
        builder.ConfigureAuditFields();
    }
}
