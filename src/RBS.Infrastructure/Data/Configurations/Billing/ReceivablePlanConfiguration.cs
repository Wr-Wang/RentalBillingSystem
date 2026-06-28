using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class ReceivablePlanConfiguration : IEntityTypeConfiguration<ReceivablePlan>
{
    public void Configure(EntityTypeBuilder<ReceivablePlan> builder)
    {
        builder.ToTable("ReceivablePlans");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ContractId).IsRequired().HasComment("合同ID");
        builder.Property(e => e.FeeCodeId).IsRequired().HasComment("费用项目ID");
        builder.Property(e => e.Period).IsRequired().HasMaxLength(7).HasComment("账期（如 2026-06）");
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("应收金额");
        builder.Property(e => e.Received).HasPrecision(18, 2).HasDefaultValue(0).HasComment("已收金额");
        builder.Property(e => e.DueDate).IsRequired().HasComment("到期日");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending").HasComment("状态（Pending/Partial/Paid/Overdue）");
        builder.HasIndex(e => new { e.ContractId, e.Period, e.FeeCodeId }).IsUnique().HasDatabaseName("IX_ReceivablePlans_Contract_Period_FeeCode");
        builder.Property(e => e.RowVersion).IsRowVersion().HasComment("乐观锁版本号");
        builder.ConfigureAuditFields();
    }
}
