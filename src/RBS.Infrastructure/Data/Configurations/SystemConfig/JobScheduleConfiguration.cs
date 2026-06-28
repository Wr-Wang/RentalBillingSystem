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
        builder.Property(e => e.CronExpression).IsRequired().HasMaxLength(100).HasComment("Cron 表达式");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.Description).HasMaxLength(500).HasComment("作业描述");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.ConfigureAuditFields();
    }
}
