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
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending").HasComment("执行状态（Pending/Running/Completed/Failed）");
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000).HasComment("错误信息");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.ConfigureAuditFields();
    }
}
