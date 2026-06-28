using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Approval;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Approval;

public class ApprovalTypeConfiguration : IEntityTypeConfiguration<ApprovalType>
{
    public void Configure(EntityTypeBuilder<ApprovalType> builder)
    {
        builder.ToTable("ApprovalTypes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("审批类型名称");
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50).HasComment("审批类型编码");
        builder.HasIndex(e => e.Code);
        builder.Property(e => e.Description).HasMaxLength(200).HasComment("审批类型描述");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.ConfigureAuditFields();
    }
}
