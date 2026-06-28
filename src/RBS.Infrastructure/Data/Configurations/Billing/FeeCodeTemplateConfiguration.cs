using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class FeeCodeTemplateConfiguration : IEntityTypeConfiguration<FeeCodeTemplate>
{
    public void Configure(EntityTypeBuilder<FeeCodeTemplate> builder)
    {
        builder.ToTable("FeeCodeTemplates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FeeCodeId).IsRequired().HasComment("费用项目ID");
        builder.Property(e => e.Description).HasMaxLength(200).HasComment("模板描述");
        builder.Property(e => e.DefaultAmount).IsRequired().HasPrecision(18, 2).HasComment("默认金额");
        builder.Property(e => e.DefaultUnitPrice).HasPrecision(18, 4).HasComment("默认单价");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.ConfigureAuditFields();
    }
}
