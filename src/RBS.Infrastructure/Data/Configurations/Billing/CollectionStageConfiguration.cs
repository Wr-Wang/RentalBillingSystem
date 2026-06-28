using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class CollectionStageConfiguration : IEntityTypeConfiguration<CollectionStage>
{
    public void Configure(EntityTypeBuilder<CollectionStage> builder)
    {
        builder.ToTable("CollectionStages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("催缴阶段名称");
        builder.Property(e => e.DaysOverdue).IsRequired().HasComment("逾期天数触发条件");
        builder.Property(e => e.SortOrder).HasDefaultValue(0).HasComment("排序号");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.ConfigureAuditFields();
    }
}
