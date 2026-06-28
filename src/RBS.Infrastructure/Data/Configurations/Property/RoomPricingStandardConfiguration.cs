using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Property;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Property;

public class RoomPricingStandardConfiguration : IEntityTypeConfiguration<RoomPricingStandard>
{
    public void Configure(EntityTypeBuilder<RoomPricingStandard> builder)
    {
        builder.ToTable("RoomPricingStandards");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RoomTypeId).IsRequired().HasComment("房型ID");
        builder.Property(e => e.FloorLevelBandId).IsRequired().HasComment("楼层级别ID");
        builder.Property(e => e.RentAmount).IsRequired().HasPrecision(18, 2).HasComment("标准租金");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => new { e.RoomTypeId, e.FloorLevelBandId, e.CompanyId }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
