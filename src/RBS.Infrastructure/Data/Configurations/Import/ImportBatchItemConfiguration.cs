using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Import;

namespace RBS.Infrastructure.Data.Configurations.Import;

public class ImportBatchItemConfiguration : IEntityTypeConfiguration<ImportBatchItem>
{
    public void Configure(EntityTypeBuilder<ImportBatchItem> builder)
    {
        builder.ToTable("ImportBatchItems");
        builder.HasKey(e => e.Id);

        // ===== TPH 鉴别器 =====
        builder.HasDiscriminator<string>("ImportType")
            .HasValue<ImportBatchItem>("Base")
            .HasValue<ImportBatchItemHousingUnit>("HousingUnit");

        // ===== 通用列 =====
        builder.Property(e => e.ImportType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.RowIndex).IsRequired();
        builder.Property(e => e.IsValid).IsRequired();
        builder.Property(e => e.ErrorCode).HasMaxLength(50);
        builder.Property(e => e.ErrorMessage).HasMaxLength(500);
        builder.Property(e => e.FixSuggestion).HasMaxLength(500);

        // ===== 索引 =====
        builder.HasIndex(e => new { e.ImportBatchId, e.IsValid });
    }
}

public class ImportBatchItemHousingUnitConfiguration : IEntityTypeConfiguration<ImportBatchItemHousingUnit>
{
    public void Configure(EntityTypeBuilder<ImportBatchItemHousingUnit> builder)
    {
        // TPH 共享 ImportBatchItems 表，基类配置已指定表名和键
        // 此处仅配置派生类特有的属性与索引

        builder.Property(e => e.BuildingName).HasMaxLength(100);
        builder.Property(e => e.BuildingCode).HasMaxLength(50);
        builder.Property(e => e.BuildingAddress).HasMaxLength(200);
        builder.Property(e => e.FloorName).HasMaxLength(50);
        builder.Property(e => e.FloorSortOrder);
        builder.Property(e => e.UnitNo).HasMaxLength(50);
        builder.Property(e => e.FullCode).HasMaxLength(200);
        builder.Property(e => e.RoomTypeId);
        builder.Property(e => e.RoomTypeName).HasMaxLength(100);
        builder.Property(e => e.Orientation).HasMaxLength(20);
        builder.Property(e => e.Area).HasColumnType("decimal(18,2)");
        builder.Property(e => e.BaseRentAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PriceWarning).HasMaxLength(500);

        // 房源导入行索引（仅 HousingUnit 类型有值）
        builder.HasIndex(e => new { e.BuildingName, e.FloorName, e.UnitNo })
            .HasFilter("[ImportType] = 'HousingUnit' AND [BuildingName] IS NOT NULL");
    }
}
