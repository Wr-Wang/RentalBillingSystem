using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractEntity = RBS.Core.Entities.Contract.Contract;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Contract;

public class ContractConfiguration : IEntityTypeConfiguration<ContractEntity>
{
    public void Configure(EntityTypeBuilder<ContractEntity> builder)
    {
        builder.ToTable("Contracts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ContractNo).IsRequired().HasMaxLength(100).HasComment("合同编号，自动生成");
        builder.HasIndex(e => e.ContractNo).IsUnique();
        builder.Property(e => e.RentAmount).IsRequired().HasPrecision(18, 2).HasComment("租金金额");
        builder.Property(e => e.DepositAmount).IsRequired().HasPrecision(18, 2).HasComment("押金金额");
        builder.Property(e => e.StartDate).IsRequired().HasComment("合同开始日期");
        builder.Property(e => e.EndDate).IsRequired().HasComment("合同结束日期");
        builder.Property(e => e.PaymentCycle).IsRequired().HasMaxLength(20).HasComment("付款周期（Monthly/Quarterly/Yearly）");
        builder.Property(e => e.StatusCode).IsRequired().HasMaxLength(20).HasColumnName("Status").HasComment("合同状态（Draft/Active/Suspended/Terminated等）");
        builder.HasIndex(e => e.StatusCode).HasDatabaseName("IX_Contracts_Status");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.HasIndex(e => e.LandlordId);
        builder.HasIndex(e => new { e.LandlordId, e.StatusCode }).HasDatabaseName("IX_Contracts_LandlordId_Status");
        builder.Property(e => e.RowVersion).IsRowVersion().HasComment("乐观锁版本号");
        builder.HasMany(e => e.ContractTenants).WithOne().HasForeignKey(e => e.ContractId);
        builder.HasMany(e => e.FeeConfigs).WithOne().HasForeignKey(e => e.ContractId);
        builder.ConfigureAuditFields();
    }
}
