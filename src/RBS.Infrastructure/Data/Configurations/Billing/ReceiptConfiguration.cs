using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ReceiptNo).IsRequired().HasMaxLength(100).HasComment("收款单号");
        builder.HasIndex(e => e.ReceiptNo).IsUnique();
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("收款金额");
        builder.Property(e => e.ReceivedDate).IsRequired().HasComment("收款日期");
        builder.Property(e => e.ReferenceNo).HasMaxLength(100).HasComment("外部参考号（银行流水号等）");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending").HasComment("状态（Pending待确认/Confirmed已确认/Rejected已驳回）");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => new { e.Status, e.CompanyId });
        builder.Property(e => e.RowVersion).IsRowVersion().HasComment("乐观锁版本号");
        builder.HasMany(e => e.Allocations).WithOne().HasForeignKey(e => e.ReceiptId);
        builder.ConfigureAuditFields();
    }
}
