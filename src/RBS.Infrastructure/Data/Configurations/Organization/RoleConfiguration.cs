using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasComment("角色名称");
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50).HasComment("角色编码（如 Admin/OpsSupervisor）");
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Description).HasMaxLength(200).HasComment("角色描述");
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasComment("是否启用");
        builder.ConfigureAuditFields();
    }
}
