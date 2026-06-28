using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class TaxRateConfigConfiguration : IEntityTypeConfiguration<TaxRateConfig>
{
    public void Configure(EntityTypeBuilder<TaxRateConfig> builder)
    {
        builder.ToTable("TaxRateConfigs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("税率名称");
        builder.Property(e => e.Rate).IsRequired().HasPrecision(5, 2).HasComment("税率（百分比）");
        builder.Property(e => e.EffectiveDate).IsRequired().HasComment("生效日期");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.ConfigureAuditFields();
    }
}
