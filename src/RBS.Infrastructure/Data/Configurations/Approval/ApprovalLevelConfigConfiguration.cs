using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Approval;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Approval;

public class ApprovalLevelConfigConfiguration : IEntityTypeConfiguration<ApprovalLevelConfig>
{
    public void Configure(EntityTypeBuilder<ApprovalLevelConfig> builder)
    {
        builder.ToTable("ApprovalLevelConfigs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ApprovalTypeId).IsRequired().HasComment("审批类型ID");
        builder.Property(e => e.Level).IsRequired().HasComment("审批级别序号");
        builder.Property(e => e.RoleId).IsRequired().HasComment("审批角色ID");
        builder.Property(e => e.MinAmount).HasPrecision(18, 2).HasComment("金额下限（满足此金额范围才需本级别审批）");
        builder.Property(e => e.MaxAmount).HasPrecision(18, 2).HasComment("金额上限");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.ConfigureAuditFields();
    }
}
