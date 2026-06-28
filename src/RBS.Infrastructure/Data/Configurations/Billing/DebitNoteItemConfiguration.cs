using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class DebitNoteItemConfiguration : IEntityTypeConfiguration<DebitNoteItem>
{
    public void Configure(EntityTypeBuilder<DebitNoteItem> builder)
    {
        builder.ToTable("DebitNoteItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DebitNoteId).IsRequired().HasComment("账单ID");
        builder.Property(e => e.FeeCodeId).IsRequired().HasComment("费用项目ID");
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("费用金额");
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
