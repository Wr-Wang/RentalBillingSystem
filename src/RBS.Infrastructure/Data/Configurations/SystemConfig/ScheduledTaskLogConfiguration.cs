using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class ScheduledTaskLogConfiguration : IEntityTypeConfiguration<ScheduledTaskLog>
{
    public void Configure(EntityTypeBuilder<ScheduledTaskLog> builder)
    {
        builder.ToTable("ScheduledTaskLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TaskName).IsRequired().HasMaxLength(200).HasComment("任务名称");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending")
            .HasComment("执行状态（Pending/Running/Completed/Failed/Stale）");
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000).HasComment("错误信息");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.Property(e => e.TargetMonth).HasMaxLength(7).HasComment("目标月份 yyyy-MM");
        builder.Property(e => e.HeartbeatAt).HasComment("心跳时间");
        builder.ConfigureAuditFields();

        // 执行锁：同公司同月同一任务只能执行一次
        builder.HasIndex(e => new { e.TaskName, e.CompanyId, e.TargetMonth })
            .IsUnique()
            .HasDatabaseName("IX_ScheduledTaskLogs_JobLock")
            .HasFilter("[TargetMonth] IS NOT NULL");
        builder.HasIndex(e => e.HeartbeatAt).HasDatabaseName("IX_ScheduledTaskLogs_HeartbeatAt");
    }
}
