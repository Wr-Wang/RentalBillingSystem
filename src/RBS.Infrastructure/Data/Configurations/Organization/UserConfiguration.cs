using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Username).IsRequired().HasMaxLength(50).HasComment("登录用户名，全局唯一");
        builder.HasIndex(e => e.Username).IsUnique();
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(200).HasComment("密码哈希值");
        builder.Property(e => e.DisplayName).IsRequired().HasMaxLength(100).HasComment("用户显示名称/姓名");
        builder.Property(e => e.Phone).HasMaxLength(20).HasComment("手机号");
        builder.Property(e => e.Email).HasMaxLength(200).HasComment("电子邮箱");
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.IsSuperAdmin).HasDefaultValue(false).HasComment("是否为超级管理员");
        builder.HasMany(e => e.Roles).WithOne().HasForeignKey(e => e.UserId);
        builder.ConfigureAuditFields();
    }
}
