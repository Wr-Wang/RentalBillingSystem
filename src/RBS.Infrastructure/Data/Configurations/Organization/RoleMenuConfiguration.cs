using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class RoleMenuConfiguration : IEntityTypeConfiguration<RoleMenu>
{
    public void Configure(EntityTypeBuilder<RoleMenu> builder)
    {
        builder.ToTable("RoleMenus");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RoleId).IsRequired().HasComment("角色ID");
        builder.Property(e => e.MenuId).IsRequired().HasComment("菜单ID");
        builder.HasIndex(e => new { e.RoleId, e.MenuId }).IsUnique();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
