using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.Billing;

public class CollectionRecordConfiguration : IEntityTypeConfiguration<CollectionRecord>
{
    public void Configure(EntityTypeBuilder<CollectionRecord> builder)
    {
        builder.ToTable("CollectionRecords");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ContractId).IsRequired().HasComment("合同ID");
        builder.Property(e => e.CollectionStageId).IsRequired().HasComment("催缴阶段ID");
        builder.Property(e => e.ContactResult).HasMaxLength(500).HasComment("联系结果");
        builder.Property(e => e.Remark).HasMaxLength(500).HasComment("备注");
        builder.ConfigureAuditFields();
    }
}
