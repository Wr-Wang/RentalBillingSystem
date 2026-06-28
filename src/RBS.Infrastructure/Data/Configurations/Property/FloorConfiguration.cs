using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Property;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Property;

public class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.ToTable("Floors");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BuildingId).IsRequired().HasComment("所属楼宇ID");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("楼层名称（如 1层、2层）");
        builder.Property(e => e.SortOrder).HasDefaultValue(0).HasComment("楼层排序号");
        builder.HasIndex(e => new { e.BuildingId, e.Name }).IsUnique();
        builder.HasMany(e => e.Rooms).WithOne().HasForeignKey(e => e.FloorId);
        builder.ConfigureAuditFields();
    }
}
