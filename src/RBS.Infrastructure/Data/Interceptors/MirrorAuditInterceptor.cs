using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RBS.Core.Common;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Interceptors;

/// <summary>
/// 镜像审计拦截器 — 每次 SaveChanges 时将变更写入 {TableName}_Audit 表
/// </summary>
public class MirrorAuditInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> ExcludedEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        "ScheduledTaskLog"
    };

    private readonly ICurrentUserService _currentUserService;

    public MirrorAuditInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var userId = _currentUserService.UserId;
        var utcNow = RBS.Core.Common.ChinaTime.Now;

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries().Where(e =>
            e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var entityType = context.Model.FindEntityType(entry.Entity.GetType());
            if (entityType == null) continue;

            var tableName = entityType.GetTableName();
            if (tableName == null || ExcludedEntities.Contains(tableName)) continue;

            // 只审计继承自 AuditableEntity 的实体（跳过关联表）
            if (entry.Entity is not Core.Entities.Base.AuditableEntity) continue;

            var auditEntry = new AuditEntry
            {
                TableName = tableName,
                EntityId = entry.Property("Id")?.CurrentValue?.ToString() ?? "",
                Action = entry.State switch
                {
                    EntityState.Added => "Insert",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => "Unknown"
                },
                Changes = new Dictionary<string, object?>()
            };

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name == "RowVersion") continue;
                auditEntry.Changes[property.Metadata.Name] = property.CurrentValue;
            }

            auditEntry.AuditChangedBy = userId;
            auditEntry.AuditChangedAt = utcNow;

            auditEntries.Add(auditEntry);
        }

        if (auditEntries.Count > 0)
        {
            await WriteAuditEntriesAsync(context, auditEntries, cancellationToken);
        }

        return result;
    }

    private async Task WriteAuditEntriesAsync(
        DbContext context,
        List<AuditEntry> entries,
        CancellationToken ct)
    {
        foreach (var entry in entries)
        {
            var columns = new List<string>
            {
                "Id", "AuditAction", "AuditVersionNo", "AuditChangedAt", "AuditChangedBy"
            };
            var values = new List<object?>
            {
                entry.EntityId,
                entry.Action,
                1, // simplified version
                entry.AuditChangedAt,
                entry.AuditChangedBy
            };

            foreach (var kv in entry.Changes)
            {
                if (kv.Key == "Id") continue;
                columns.Add($"[{kv.Key}]");
                values.Add(kv.Value);
            }

            var paramNames = values.Select((_, i) => $"@p{i}");
            var sql = $"INSERT INTO [{entry.TableName}_Audit] ({string.Join(", ", columns)}) " +
                      $"VALUES ({string.Join(", ", paramNames)})";

            try
            {
                await context.Database.ExecuteSqlRawAsync(sql, values.ToArray()!, ct);
            }
            catch
            {
                // 审计写入失败不应阻塞主操作
            }
        }
    }

    internal class AuditEntry
    {
        public string TableName { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string Action { get; set; } = "";
        public Dictionary<string, object?> Changes { get; set; } = new();
        public Guid AuditChangedBy { get; set; }
        public DateTime AuditChangedAt { get; set; }
    }
}
