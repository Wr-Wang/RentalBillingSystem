using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Property;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Property;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200).HasComment("楼宇名称");
        builder.Property(e => e.Code).HasMaxLength(50).HasComment("楼宇编码");
        builder.Property(e => e.Address).HasMaxLength(500).HasComment("楼宇地址");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => e.CompanyId);
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.HasMany(e => e.Floors).WithOne().HasForeignKey(e => e.BuildingId);
        builder.ConfigureAuditFields();
    }
}
