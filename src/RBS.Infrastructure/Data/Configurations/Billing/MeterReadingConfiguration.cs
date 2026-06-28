using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.ToTable("MeterReadings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ContractFeeConfigId).IsRequired().HasComment("合同费用配置ID");
        builder.Property(e => e.Year).IsRequired().HasComment("抄表年份");
        builder.Property(e => e.Month).IsRequired().HasComment("抄表月份");
        builder.Property(e => e.PreviousReading).HasPrecision(18, 4).HasComment("上次读数");
        builder.Property(e => e.CurrentReading).HasPrecision(18, 4).HasComment("本次读数");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Draft").HasComment("状态（Draft草稿/Confirmed已确认）");
        builder.HasIndex(e => new { e.ContractFeeConfigId, e.Year, e.Month }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
