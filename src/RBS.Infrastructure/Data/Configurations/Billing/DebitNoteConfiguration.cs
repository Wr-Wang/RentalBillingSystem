using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class DebitNoteConfiguration : IEntityTypeConfiguration<DebitNote>
{
    public void Configure(EntityTypeBuilder<DebitNote> builder)
    {
        builder.ToTable("DebitNotes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.NoteNo).IsRequired().HasMaxLength(100).HasComment("账单编号");
        builder.HasIndex(e => e.NoteNo).IsUnique();
        builder.Property(e => e.ContractId).IsRequired().HasComment("合同ID");
        builder.Property(e => e.Period).IsRequired().HasMaxLength(7).HasComment("账单账期");
        builder.Property(e => e.TotalAmount).IsRequired().HasPrecision(18, 2).HasComment("账单总金额");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Draft").HasComment("状态（Draft草稿/Issued已发布）");
        builder.HasMany(e => e.Items).WithOne().HasForeignKey(e => e.DebitNoteId);
        builder.ConfigureAuditFields();
    }
}
