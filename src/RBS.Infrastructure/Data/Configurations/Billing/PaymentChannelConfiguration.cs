using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class PaymentChannelConfiguration : IEntityTypeConfiguration<PaymentChannel>
{
    public void Configure(EntityTypeBuilder<PaymentChannel> builder)
    {
        builder.ToTable("PaymentChannels");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("支付通道名称");
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50).HasComment("支付通道编码");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => new { e.Code, e.CompanyId }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
