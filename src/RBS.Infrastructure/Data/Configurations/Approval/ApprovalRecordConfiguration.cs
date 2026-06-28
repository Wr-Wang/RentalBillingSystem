using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Approval;

namespace RBS.Infrastructure.Data.Configurations.Approval;

public class ApprovalRecordConfiguration : IEntityTypeConfiguration<ApprovalRecord>
{
    public void Configure(EntityTypeBuilder<ApprovalRecord> builder)
    {
        builder.ToTable("ApprovalRecords");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ApprovalRequestId).IsRequired().HasComment("审批请求ID");
        builder.Property(e => e.Level).IsRequired().HasComment("审批级别");
        builder.Property(e => e.ApproverId).IsRequired().HasComment("审批人ID");
        builder.Property(e => e.Action).IsRequired().HasMaxLength(20).HasComment("审批动作（Approved通过/Rejected驳回）");
        builder.Property(e => e.Comment).HasMaxLength(500).HasComment("审批意见");
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
