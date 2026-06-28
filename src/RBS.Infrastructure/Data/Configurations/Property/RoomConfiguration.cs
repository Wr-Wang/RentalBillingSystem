using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Property;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Property;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BuildingId).IsRequired().HasComment("所属楼宇ID");
        builder.Property(e => e.FloorId).IsRequired().HasComment("所属楼层ID");
        builder.Property(e => e.RoomNo).IsRequired().HasMaxLength(50).HasComment("房间编号（如 101）");
        builder.Property(e => e.FullCode).HasMaxLength(100).HasComment("房间完整编码（如 A栋-1层-101）");
        builder.HasIndex(e => e.FullCode);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20)
            .HasDefaultValue(RoomStatus.Vacant)
            .HasConversion(ValueConverters.RoomStatusConverter)
            .HasComment("房间状态：Vacant空置/Rented已租/Maintenance维修");
        builder.Property(e => e.Area).HasPrecision(10, 2).HasComment("房间面积（平方米）");
        builder.HasIndex(e => new { e.BuildingId, e.Status });
        builder.ConfigureAuditFields();
    }
}
