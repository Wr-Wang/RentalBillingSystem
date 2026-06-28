using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Property;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Property;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomTypes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("房型名称（整租/合租等）");
        builder.Property(e => e.Description).HasMaxLength(200).HasComment("房型描述");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.ConfigureAuditFields();
    }
}
