using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Approval;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Approval;

public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("ApprovalRequests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ApprovalTypeId).IsRequired().HasComment("审批类型ID");
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200).HasComment("审批标题");
        builder.Property(e => e.Description).HasMaxLength(1000).HasComment("审批申请描述");
        builder.Property(e => e.TargetEntityId).IsRequired().HasComment("目标业务实体ID");
        builder.Property(e => e.TargetEntityType).IsRequired().HasMaxLength(50).HasComment("目标业务实体类型");
        builder.Property(e => e.CurrentLevel).HasDefaultValue(1).HasComment("当前审批级别");
        builder.Property(e => e.MaxLevel).HasDefaultValue(1).HasComment("最大审批级别");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending").IsConcurrencyToken().HasComment("审批状态（Pending/Approved/Rejected/Cancelled）");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => new { e.CompanyId, e.Status });
        builder.Property(e => e.RowVersion).IsRequired().ValueGeneratedOnAddOrUpdate().HasColumnType("rowversion").HasComment("乐观锁版本号").IsConcurrencyToken(false);
        builder.HasMany(e => e.Records).WithOne().HasForeignKey(e => e.ApprovalRequestId);
        builder.ConfigureAuditFields();
    }
}
