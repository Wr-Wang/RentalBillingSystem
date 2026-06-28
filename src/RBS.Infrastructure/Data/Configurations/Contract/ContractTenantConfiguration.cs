using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Contract;

namespace RBS.Infrastructure.Data.Configurations.Contract;

public class ContractTenantConfiguration : IEntityTypeConfiguration<ContractTenant>
{
    public void Configure(EntityTypeBuilder<ContractTenant> builder)
    {
        builder.ToTable("ContractTenants");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ContractId).IsRequired().HasComment("合同ID");
        builder.Property(e => e.TenantId).IsRequired().HasComment("租客ID");
        builder.Property(e => e.IsPrimary).HasDefaultValue(false).HasComment("是否主租客");
        builder.HasIndex(e => new { e.ContractId, e.TenantId }).IsUnique();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
