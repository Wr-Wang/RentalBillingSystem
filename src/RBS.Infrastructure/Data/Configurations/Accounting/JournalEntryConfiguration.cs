using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Accounting;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.VoucherId).IsRequired().HasComment("凭证ID");
        builder.Property(e => e.AccountingSubjectId).IsRequired().HasComment("会计科目ID");
        builder.Property(e => e.Direction).IsRequired().HasMaxLength(10).HasComment("借贷方向（Debit/Credit）");
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("金额");
        builder.Property(e => e.Summary).HasMaxLength(200).HasComment("分录摘要说明");
        builder.ConfigureAuditFields();
    }
}
