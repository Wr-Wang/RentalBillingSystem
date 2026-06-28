using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Contract;

public class ContractFeeConfigConfiguration : IEntityTypeConfiguration<ContractFeeConfig>
{
    public void Configure(EntityTypeBuilder<ContractFeeConfig> builder)
    {
        builder.ToTable("ContractFeeConfigs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ContractId).IsRequired().HasComment("合同ID");
        builder.Property(e => e.FeeCodeId).IsRequired().HasComment("费用项目ID");
        builder.Property(e => e.BillingMode).IsRequired().HasMaxLength(20)
            .HasDefaultValue(BillingMode.FixedAmount)
            .HasConversion(ValueConverters.BillingModeConverter).HasComment("计费模式（FixedAmount固定金额/MeterBased抄表）");
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("金额");
        builder.Property(e => e.Unit).HasMaxLength(20).HasComment("计量单位");
        builder.Property(e => e.UnitPrice).HasPrecision(18, 4).HasComment("单价");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.HasIndex(e => new { e.ContractId, e.FeeCodeId }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
