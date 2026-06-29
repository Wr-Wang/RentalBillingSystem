using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class ApiLogConfiguration : IEntityTypeConfiguration<ApiLog>
{
    public void Configure(EntityTypeBuilder<ApiLog> builder)
    {
        builder.ToTable("ApiLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.HttpMethod).IsRequired().HasMaxLength(10).HasComment("HTTP 方法");
        builder.Property(e => e.Path).IsRequired().HasMaxLength(500).HasComment("请求路径");
        builder.Property(e => e.QueryString).HasMaxLength(2000).HasComment("查询参数");
        builder.Property(e => e.RequestBody).HasComment("请求体");
        builder.Property(e => e.StatusCode).IsRequired().HasComment("HTTP 状态码");
        builder.Property(e => e.ResponseBody).HasComment("响应体");
        builder.Property(e => e.DurationMs).IsRequired().HasComment("耗时(ms)");
        builder.Property(e => e.IpAddress).HasMaxLength(50).HasComment("客户端IP");
        builder.Property(e => e.UserAgent).HasMaxLength(500).HasComment("用户代理");
        builder.Property(e => e.UserDisplayName).HasMaxLength(100).HasComment("用户显示名");
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()").HasComment("创建时间");

        builder.HasIndex(e => e.CreatedAt).IsDescending();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Path);
    }
}
