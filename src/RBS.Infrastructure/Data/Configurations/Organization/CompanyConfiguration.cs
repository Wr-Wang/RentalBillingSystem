using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200).HasComment("公司名称");
        builder.Property(e => e.Code).HasMaxLength(50).HasComment("公司编码");
        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        builder.Property(e => e.ContactPerson).HasMaxLength(100).HasComment("联系人");
        builder.Property(e => e.Phone).HasMaxLength(20).HasComment("联系电话");
        builder.Property(e => e.Address).HasMaxLength(500).HasComment("通讯地址");
        builder.Property(e => e.IdType).HasMaxLength(30).HasComment("证件类型");
        builder.Property(e => e.IdNumber).HasMaxLength(50).HasComment("证件号码");
        builder.Property(e => e.BankName).HasMaxLength(200).HasComment("开户行");
        builder.Property(e => e.BankAccount).HasMaxLength(50).HasComment("银行账号");
        builder.Property(e => e.BankAccountName).HasMaxLength(100).HasComment("开户名");
        builder.Property(e => e.SettlementCycle).HasMaxLength(20).HasComment("结算周期");
        builder.Property(e => e.SettlementDay).HasComment("结算日");
        builder.Property(e => e.CommissionRate).HasColumnType("decimal(5,2)").HasComment("佣金比例(%)");
        builder.Property(e => e.Remark).HasMaxLength(500).HasComment("备注");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.ConfigureAuditFields();
    }
}
