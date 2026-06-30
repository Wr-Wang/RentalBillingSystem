using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Base; using RBS.Core.Entities.Property; using RBS.Infrastructure.Data.Extensions;
namespace RBS.Infrastructure.Data.Configurations.Property;
public class HousingUnitConfiguration : IEntityTypeConfiguration<HousingUnit>
{
    public void Configure(EntityTypeBuilder<HousingUnit> builder)
    {
        builder.ToTable("HousingUnits"); builder.HasKey(e => e.Id);
        builder.Property(e => e.BuildingName).IsRequired().HasMaxLength(200).HasComment("座楼名称");
        builder.Property(e => e.BuildingCode).HasMaxLength(50).HasComment("座楼编码");
        builder.Property(e => e.BuildingAddress).HasMaxLength(500).HasComment("地址");
        builder.Property(e => e.CompanyId).IsRequired().HasComment("所属公司"); builder.HasIndex(e => e.CompanyId);
        builder.Property(e => e.FloorName).IsRequired().HasMaxLength(100).HasComment("楼层名称");
        builder.Property(e => e.FloorSortOrder).HasDefaultValue(0).HasComment("楼层排序");
        builder.Property(e => e.UnitNo).IsRequired().HasMaxLength(50).HasComment("房源编号");
        builder.Property(e => e.FullCode).HasMaxLength(100).HasComment("完整编码");
        builder.HasIndex(e => e.FullCode).IsUnique().HasFilter("[FullCode] IS NOT NULL");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue(RoomStatus.Vacant).HasConversion(ValueConverters.RoomStatusConverter).HasComment("状态");
        builder.Property(e => e.Area).HasPrecision(10,2).HasComment("面积");
        builder.Property(e => e.Orientation).HasMaxLength(20).HasComment("朝向");
        builder.Property(e => e.BaseRentAmount).HasPrecision(10,2).HasComment("基础月租金");
        builder.HasIndex(e => new { e.BuildingName, e.FloorName, e.UnitNo }).IsUnique();
        builder.HasIndex(e => new { e.BuildingName, e.Status });
        builder.ConfigureAuditFields();
    }
}
