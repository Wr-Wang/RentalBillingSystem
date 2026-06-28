using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class ReceiptAllocationConfiguration : IEntityTypeConfiguration<ReceiptAllocation>
{
    public void Configure(EntityTypeBuilder<ReceiptAllocation> builder)
    {
        builder.ToTable("ReceiptAllocations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ReceiptId).IsRequired().HasComment("收款单ID");
        builder.Property(e => e.ReceivablePlanId).IsRequired().HasComment("应收计划ID");
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("分配金额");
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
