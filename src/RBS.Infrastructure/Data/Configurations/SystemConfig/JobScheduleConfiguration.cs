using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class JobScheduleConfiguration : IEntityTypeConfiguration<JobSchedule>
{
    public void Configure(EntityTypeBuilder<JobSchedule> builder)
    {
        builder.ToTable("JobSchedules");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.JobName).IsRequired().HasMaxLength(200).HasComment("作业名称");
        builder.Property(e => e.ScheduleType).IsRequired().HasMaxLength(10).HasDefaultValue("Monthly").HasComment("调度类型: Daily/Monthly");
        builder.Property(e => e.Hour).IsRequired().HasComment("执行小时 0~23");
        builder.Property(e => e.Minute).IsRequired().HasComment("执行分钟 0~59");
        builder.Property(e => e.DayOfMonth).HasComment("执行日 1~31（Monthly 类型使用）");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.Description).HasMaxLength(500).HasComment("作业描述");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.Property(e => e.TemplateCode).HasMaxLength(50).HasComment("创建来源模板代码");
        builder.Property(e => e.LastRunAt).HasComment("上次执行时间");
        builder.Property(e => e.LastRunStatus).HasMaxLength(20).HasComment("上次执行结果");
        builder.ConfigureAuditFields();
    }
}
