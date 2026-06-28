using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class FeeCodeConfiguration : IEntityTypeConfiguration<FeeCode>
{
    public void Configure(EntityTypeBuilder<FeeCode> builder)
    {
        builder.ToTable("FeeCodes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50).HasComment("费用编码（如 RENT/WATER/ELECTRIC）");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("费用名称（如 房租费/水费/电费）");
        builder.Property(e => e.BillingMode).IsRequired().HasMaxLength(20).HasDefaultValue("FixedAmount").HasComment("计费模式");
        builder.Property(e => e.Unit).HasMaxLength(20).HasComment("计量单位（元/吨、元/度）");
        builder.Property(e => e.SortOrder).HasDefaultValue(0).HasComment("排序号");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("Other").HasComment("费用分类（Core核心/Utility公共事业）");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => new { e.Code, e.CompanyId }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
