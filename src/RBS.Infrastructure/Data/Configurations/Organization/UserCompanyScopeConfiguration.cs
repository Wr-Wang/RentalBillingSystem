using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class UserCompanyScopeConfiguration : IEntityTypeConfiguration<UserCompanyScope>
{
    public void Configure(EntityTypeBuilder<UserCompanyScope> builder)
    {
        builder.ToTable("UserCompanyScopes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired().HasComment("用户ID");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("公司ID，用户可操作的数据范围");
        builder.HasIndex(e => new { e.UserId, e.CompanyId }).IsUnique();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
