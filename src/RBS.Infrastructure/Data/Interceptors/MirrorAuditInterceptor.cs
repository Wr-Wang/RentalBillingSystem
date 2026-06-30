using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RBS.Core.Common;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Interceptors;

/// <summary>
/// 镜像审计拦截器 — 每次 SaveChanges 时将变更写入 {TableName}_Audit 表
/// 写前检查表是否存在，避免因缺失 _Audit 表导致事务破坏（修复：静默 catch 吞异常后事务被污染的问题）
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
            await WriteAuditEntriesSafeAsync(context, auditEntries, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// 安全写入审计表：先 IF EXISTS 检查表是否存在，不存在则跳过。
    /// 避免因缺失 _Audit 表导致 SQL 错误进而破坏当前事务。
    /// </summary>
    private async Task WriteAuditEntriesSafeAsync(
        DbContext context,
        List<AuditEntry> entries,
        CancellationToken ct)
    {
        foreach (var entry in entries)
        {
            var auditTableName = $"{entry.TableName}_Audit";

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

            // 单条 SQL：先检查表是否存在，存在才 INSERT
            // 避免了在拦截器中执行 INSERT 到不存在的表导致事务被破坏
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@tableName", auditTableName)
            };
            for (int i = 0; i < values.Count; i++)
            {
                parameters.Add(new SqlParameter($"@p{i}", values[i] ?? DBNull.Value));
            }

            var fullSql = $@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = @tableName)
    INSERT INTO [{auditTableName}] ({string.Join(", ", columns)})
    VALUES ({string.Join(", ", paramNames)})";

            try
            {
                await context.Database.ExecuteSqlRawAsync(fullSql, parameters, ct);
            }
            catch
            {
                // 安全网：即便 IF EXISTS 检查后仍有失败（极少见），也不应阻塞主操作
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
