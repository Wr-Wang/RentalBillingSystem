namespace RBS.Infrastructure.Data.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBS.Core.Entities.Base;

/// <summary>
/// ModelBuilder 扩展方法 — 全局级联删除禁用
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// 禁用所有外键的级联删除（逻辑外键策略）
    /// </summary>
    public static void UseLogicalForeignKeys(this ModelBuilder modelBuilder)
    {
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }

    /// <summary>
    /// 审计字段配置扩展
    /// </summary>
    public static void ConfigureAuditFields<T>(this EntityTypeBuilder<T> builder)
        where T : AuditableEntity
    {
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");
        builder.HasIndex(e => e.CreatedAt);

        builder.Property(e => e.CreatedIp).HasMaxLength(50);
        builder.Property(e => e.CreatedHostname).HasMaxLength(100);

        builder.Property(e => e.UpdatedBy);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.UpdatedIp).HasMaxLength(50);
        builder.Property(e => e.UpdatedHostname).HasMaxLength(100);
    }
}
