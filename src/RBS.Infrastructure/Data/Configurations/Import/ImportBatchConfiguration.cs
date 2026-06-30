using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Import;

namespace RBS.Infrastructure.Data.Configurations.Import;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ImportType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);

        builder.HasMany(e => e.Items)
               .WithOne(e => e.Batch)
               .HasForeignKey(e => e.ImportBatchId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CompanyId, e.Status });
        builder.HasIndex(e => new { e.ImportType, e.Status });
    }
}
