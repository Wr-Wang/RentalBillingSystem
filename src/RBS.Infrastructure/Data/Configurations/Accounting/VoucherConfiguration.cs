using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Accounting;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Accounting;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.VoucherNo).IsRequired().HasMaxLength(100).HasComment("凭证编号");
        builder.HasIndex(e => e.VoucherNo).IsUnique();
        builder.Property(e => e.VoucherDate).IsRequired().HasComment("凭证日期");
        builder.Property(e => e.Description).HasMaxLength(500).HasComment("凭证摘要");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20)
            .HasDefaultValue(VoucherStatus.Draft)
            .HasConversion(ValueConverters.VoucherStatusConverter)
            .HasComment("凭证状态（Draft草稿/Posted已过账/Audited已审核）");
        builder.Property(e => e.SourceEntityType).HasMaxLength(50).HasComment("来源业务实体类型");
        builder.Property(e => e.RowVersion).IsRowVersion().HasComment("乐观锁版本号");
        builder.HasMany(e => e.Entries).WithOne().HasForeignKey(e => e.VoucherId);
        builder.ConfigureAuditFields();
    }
}
