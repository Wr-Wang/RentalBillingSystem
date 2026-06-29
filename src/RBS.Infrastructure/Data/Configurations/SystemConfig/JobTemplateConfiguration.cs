using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class JobTemplateConfiguration : IEntityTypeConfiguration<JobTemplate>
{
    public void Configure(EntityTypeBuilder<JobTemplate> builder)
    {
        builder.ToTable("JobTemplates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50).HasComment("模板代码");
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.DisplayName).IsRequired().HasMaxLength(100).HasComment("显示名");
        builder.Property(e => e.ShortName).IsRequired().HasMaxLength(50).HasComment("短名");
        builder.Property(e => e.DefaultScheduleType).IsRequired().HasMaxLength(10).HasDefaultValue("Monthly").HasComment("默认调度类型");
        builder.Property(e => e.DefaultHour).IsRequired().HasComment("默认小时");
        builder.Property(e => e.DefaultMinute).IsRequired().HasComment("默认分钟");
        builder.Property(e => e.DefaultDayOfMonth).HasComment("默认执行日（Monthly类型）");
        builder.Property(e => e.Description).HasMaxLength(500).HasComment("模板说明");
        builder.Property(e => e.Icon).HasMaxLength(50).HasComment("前端图标");
        builder.Property(e => e.Category).IsRequired().HasMaxLength(50).HasComment("分类");
        builder.Property(e => e.SortOrder).HasDefaultValue(0).HasComment("排序号");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
    }
}
