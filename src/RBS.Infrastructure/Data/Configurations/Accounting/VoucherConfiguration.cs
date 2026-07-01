using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Accounting;

namespace RBS.Infrastructure.Data.Configurations.Accounting;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue("Draft");
        builder.Property(e => e.VoucherNo).IsRequired().HasMaxLength(100);
        builder.Property(e => e.VoucherDate).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.SourceEntityId);
        builder.Property(e => e.SourceEntityType).HasMaxLength(50);
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
