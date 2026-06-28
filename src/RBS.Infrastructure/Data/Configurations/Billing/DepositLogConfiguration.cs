using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class DepositLogConfiguration : IEntityTypeConfiguration<DepositLog>
{
    public void Configure(EntityTypeBuilder<DepositLog> builder)
    {
        builder.ToTable("DepositLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ContractId).IsRequired().HasComment("合同ID");
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("押金变动金额");
        builder.Property(e => e.Balance).IsRequired().HasPrecision(18, 2).HasComment("押金余额");
        builder.Property(e => e.Action).IsRequired().HasMaxLength(20).HasDefaultValue("Create").HasComment("操作类型（Create创建/Return退还/Deduct扣除）");
        builder.Property(e => e.Remark).HasMaxLength(500).HasComment("备注说明");
        builder.ConfigureAuditFields();
    }
}
