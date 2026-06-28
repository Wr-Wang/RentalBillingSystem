using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class LandlordConfiguration : IEntityTypeConfiguration<Landlord>
{
    public void Configure(EntityTypeBuilder<Landlord> builder)
    {
        builder.ToTable("Landlords");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200).HasComment("房东/租户名称");
        builder.Property(e => e.Code).HasMaxLength(50).HasComment("房东编码");
        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        builder.Property(e => e.ContactPerson).HasMaxLength(100).HasComment("联系人");
        builder.Property(e => e.Phone).HasMaxLength(20).HasComment("联系电话");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.ConfigureAuditFields();
    }
}
