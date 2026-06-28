using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Organization;

namespace RBS.Infrastructure.Data.Configurations.Organization;

public class UserLandlordScopeConfiguration : IEntityTypeConfiguration<UserLandlordScope>
{
    public void Configure(EntityTypeBuilder<UserLandlordScope> builder)
    {
        builder.ToTable("UserLandlordScopes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired().HasComment("用户ID");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("房东ID，用户可操作的数据范围");
        builder.HasIndex(e => new { e.UserId, e.LandlordId }).IsUnique();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
