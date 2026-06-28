using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.ToTable("SystemLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Level).IsRequired().HasMaxLength(20).HasComment("日志级别");
        builder.Property(e => e.Message).HasMaxLength(2000).HasComment("日志消息");
        builder.Property(e => e.Exception).HasComment("异常堆栈");
        builder.Property(e => e.Source).HasMaxLength(200).HasComment("来源");
        builder.Property(e => e.Path).HasMaxLength(500).HasComment("请求路径");
        builder.Property(e => e.Method).HasMaxLength(10).HasComment("请求方法");
        builder.Property(e => e.IpAddress).HasMaxLength(50).HasComment("客户端IP");
        builder.Property(e => e.UserAgent).HasMaxLength(500).HasComment("用户代理");
        builder.Property(e => e.UserDisplayName).HasMaxLength(100).HasComment("用户显示名称");
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()").HasComment("创建时间");
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Level);
    }
}
