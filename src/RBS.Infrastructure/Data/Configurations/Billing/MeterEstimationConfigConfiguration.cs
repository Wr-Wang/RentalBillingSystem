using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class MeterEstimationConfigConfiguration : IEntityTypeConfiguration<MeterEstimationConfig>
{
    public void Configure(EntityTypeBuilder<MeterEstimationConfig> builder)
    {
        builder.ToTable("MeterEstimationConfigs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FeeCodeId).IsRequired().HasComment("费用项目ID");
        builder.Property(e => e.EstimatedUsage).HasPrecision(18, 4).HasComment("预估用量");
        builder.Property(e => e.Remark).HasMaxLength(200).HasComment("备注说明");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.ConfigureAuditFields();
    }
}
