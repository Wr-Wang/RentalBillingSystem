using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Contract;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Contract;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("租客姓名");
        builder.Property(e => e.IdCard).HasMaxLength(18).HasComment("身份证号");
        builder.Property(e => e.Phone).HasMaxLength(20).HasComment("联系电话");
        builder.HasIndex(e => e.Phone);
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.HasIndex(e => e.LandlordId);
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.ConfigureAuditFields();
    }
}
