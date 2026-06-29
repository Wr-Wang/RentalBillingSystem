using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class JobScheduleExecutionConfiguration : IEntityTypeConfiguration<JobScheduleExecution>
{
    public void Configure(EntityTypeBuilder<JobScheduleExecution> builder)
    {
        builder.ToTable("JobScheduleExecutions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.JobScheduleId).IsRequired().HasComment("所属任务定义ID");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.Property(e => e.TargetDate).IsRequired().HasComment("排期执行时间");
        builder.Property(e => e.OriginalDate).HasComment("原始 Cron 时间");
        builder.Property(e => e.Month).IsRequired().HasMaxLength(7).HasComment("yyyy-MM");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending").HasComment("状态");
        builder.Property(e => e.Reason).HasMaxLength(500).HasComment("备注/调整原因");
        builder.Property(e => e.IsAdjusted).HasDefaultValue(false).HasComment("是否手动调整");
        builder.Property(e => e.IsCustom).HasDefaultValue(false).HasComment("是否自定义排期");
        builder.Property(e => e.UpdatedAt).HasComment("更新时间");
        builder.ConfigureAuditFields();

        builder.HasIndex(e => new { e.JobScheduleId, e.Month })
            .IsUnique()
            .HasDatabaseName("IX_Executions_JobScheduleId_Month")
            .HasFilter("[IsCustom] = 0");
        builder.HasIndex(e => e.JobScheduleId).HasDatabaseName("IX_Executions_JobScheduleId");
        builder.HasIndex(e => e.CompanyId).HasDatabaseName("IX_Executions_CompanyId");
        builder.HasIndex(e => e.TargetDate).HasDatabaseName("IX_Executions_TargetDate");
    }
}
