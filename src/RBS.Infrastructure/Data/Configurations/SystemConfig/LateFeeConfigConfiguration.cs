using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class LateFeeConfigConfiguration : IEntityTypeConfiguration<LateFeeConfig>
{
    public void Configure(EntityTypeBuilder<LateFeeConfig> builder)
    {
        builder.ToTable("LateFeeConfigs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DailyRate).IsRequired().HasPrecision(7, 5).HasComment("日利率（如 0.0005 表示日息万分之五）");
        builder.Property(e => e.GraceDays).IsRequired().HasDefaultValue(0).HasComment("宽限天数");
        builder.Property(e => e.MaxRate).HasPrecision(5, 2).HasComment("滞纳金上限百分比");
        builder.Property(e => e.MinAmount).HasPrecision(18, 2).HasComment("最低滞纳金金额");
        builder.Property(e => e.EffectiveDate).IsRequired().HasComment("生效日期");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.ConfigureAuditFields();
    }
}
