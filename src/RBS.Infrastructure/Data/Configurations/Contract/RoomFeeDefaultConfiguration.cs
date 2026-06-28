using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Contract;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Contract;

public class RoomFeeDefaultConfiguration : IEntityTypeConfiguration<RoomFeeDefault>
{
    public void Configure(EntityTypeBuilder<RoomFeeDefault> builder)
    {
        builder.ToTable("RoomFeeDefaults");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RoomId).IsRequired().HasComment("房间ID");
        builder.Property(e => e.FeeCodeId).IsRequired().HasComment("费用项目ID");
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2).HasComment("默认金额");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司ID");
        builder.HasIndex(e => new { e.RoomId, e.FeeCodeId }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
