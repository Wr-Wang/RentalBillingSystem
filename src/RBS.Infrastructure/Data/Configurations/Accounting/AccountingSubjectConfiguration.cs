using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Accounting;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Accounting;

public class AccountingSubjectConfiguration : IEntityTypeConfiguration<AccountingSubject>
{
    public void Configure(EntityTypeBuilder<AccountingSubject> builder)
    {
        builder.ToTable("AccountingSubjects");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(20).HasComment("科目编码");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("科目名称");
        builder.Property(e => e.ParentCode).HasMaxLength(20).HasComment("父级科目编码");
        builder.Property(e => e.Level).HasDefaultValue(1).HasComment("科目层级");
        builder.Property(e => e.Direction).IsRequired().HasMaxLength(10).HasDefaultValue("Debit").HasComment("借贷方向（Debit借方/Credit贷方）");
        builder.Property(e => e.IsLeaf).HasDefaultValue(true).HasComment("是否末级科目");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.HasIndex(e => new { e.Code, e.LandlordId }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
