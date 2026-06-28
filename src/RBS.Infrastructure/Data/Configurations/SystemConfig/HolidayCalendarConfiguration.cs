using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.Extensions;

namespace RBS.Infrastructure.Data.Configurations.SystemConfig;

public class HolidayCalendarConfiguration : IEntityTypeConfiguration<HolidayCalendar>
{
    public void Configure(EntityTypeBuilder<HolidayCalendar> builder)
    {
        builder.ToTable("HolidayCalendars");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.HolidayDate).IsRequired().HasComment("日期");
        builder.Property(e => e.Name).HasMaxLength(100).HasComment("节假日名称");
        builder.Property(e => e.IsWorkingDay).HasDefaultValue(false).HasComment("是否工作日（false=放假/true=调休上班）");
        builder.Property(e => e.LandlordId).IsRequired().HasComment("所属房东ID");
        builder.HasIndex(e => new { e.HolidayDate, e.LandlordId }).IsUnique();
        builder.ConfigureAuditFields();
    }
}
