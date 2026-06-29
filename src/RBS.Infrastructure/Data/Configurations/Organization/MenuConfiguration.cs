using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("菜单名称");
        builder.Property(e => e.PermissionCode).HasMaxLength(100).HasComment("权限代码，用于接口鉴权");
        builder.Property(e => e.Path).HasMaxLength(200).HasComment("前端路由路径");
        builder.Property(e => e.Icon).HasMaxLength(50).HasComment("菜单图标类名");
        builder.Property(e => e.SortOrder).HasDefaultValue(0).HasComment("排序号");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.Property(e => e.Scope).IsRequired().HasMaxLength(10).HasDefaultValue("Company").HasComment("可见范围: Company(公司级,数据隔离) / System(仅超管)");
        builder.ConfigureAuditFields();
    }
}
