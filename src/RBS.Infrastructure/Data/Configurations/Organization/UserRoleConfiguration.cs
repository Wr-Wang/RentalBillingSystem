using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired().HasComment("用户ID");
        builder.Property(e => e.RoleId).IsRequired().HasComment("角色ID");
        builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
