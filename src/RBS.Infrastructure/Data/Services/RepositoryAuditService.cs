using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计装饰器 — 统一封装所有仓储的审计写入逻辑
/// 各仓储不再直接调用 IAuditLogWriter，全部委托给此类
/// </summary>
public class RepositoryAuditService
{
    private readonly IAuditLogWriter _auditWriter;
    private readonly IClientInfoService? _clientInfo;

    public RepositoryAuditService(IAuditLogWriter auditWriter, IClientInfoService? clientInfo = null)
    {
        _auditWriter = auditWriter;
        _clientInfo = clientInfo;
    }

    /// <summary>写入创建审计 + 填充 CreatedIp/CreatedHostname</summary>
    public async Task WriteCreateAsync<T>(string tableName, T entity, CancellationToken ct = default)
    {
        // 从请求上下文填充 CreatedIp/CreatedHostname
        if (_clientInfo != null)
        {
            TrySetProperty(entity, "CreatedIp", _clientInfo.GetClientIp());
            TrySetProperty(entity, "CreatedHostname", _clientInfo.GetClientHostname());
        }

        var dict = EntityToDict(entity);
        var createdBy = TryGetGuid(entity!, "CreatedBy") ?? Guid.Empty;
        await _auditWriter.LogChangesAsync(tableName, GetEntityId(entity), "Create", dict, createdBy, ct);
    }

    /// <summary>写入更新审计 + 填充 UpdatedIp/UpdatedHostname</summary>
    public async Task WriteUpdateAsync<T>(string tableName, T entity, CancellationToken ct = default)
    {
        var dict = EntityToDict(entity);
        // 从请求上下文填充 UpdatedIp/UpdatedHostname
        if (_clientInfo != null)
        {
            dict["UpdatedIp"] = _clientInfo.GetClientIp();
            dict["UpdatedHostname"] = _clientInfo.GetClientHostname();
        }
        var updatedBy = TryGetGuid(entity!, "UpdatedBy");
        if (updatedBy.GetValueOrDefault() == Guid.Empty)
            updatedBy = TryGetGuid(entity!, "CreatedBy");
        await _auditWriter.LogChangesAsync(tableName, GetEntityId(entity), "Update", dict, updatedBy.GetValueOrDefault(), ct);
    }

    /// <summary>写入删除审计 + 填充 UpdatedIp/UpdatedHostname</summary>
    public async Task WriteDeleteAsync<T>(string tableName, T entity, CancellationToken ct = default)
    {
        var dict = EntityToDict(entity);
        if (_clientInfo != null)
        {
            dict["UpdatedIp"] = _clientInfo.GetClientIp();
            dict["UpdatedHostname"] = _clientInfo.GetClientHostname();
        }
        var deletedBy = TryGetGuid(entity!, "UpdatedBy");
        if (deletedBy.GetValueOrDefault() == Guid.Empty)
            deletedBy = TryGetGuid(entity!, "CreatedBy");
        await _auditWriter.LogChangesAsync(tableName, GetEntityId(entity), "Delete", dict, deletedBy.GetValueOrDefault(), ct);
    }

    /// <summary>获取客户端 IP/主机名（供 UoW CommitAsync 使用）</summary>
    public (string? ip, string? hostname) GetClientInfo()
        => (_clientInfo?.GetClientIp(), _clientInfo?.GetClientHostname());

    // ===== 内部工具方法 =====

    private static string GetEntityId<T>(T entity)
    {
        return typeof(T).GetProperty("Id")?.GetValue(entity)?.ToString() ?? Guid.Empty.ToString();
    }

    private static Guid? TryGetGuid(object entity, string propName)
    {
        var val = entity.GetType().GetProperty(propName)?.GetValue(entity);
        return val is Guid g ? g : null;
    }

    private static void TrySetProperty<T>(T entity, string propName, string? value)
    {
        if (entity is null) return;
        var prop = entity.GetType().GetProperty(propName);
        if (prop != null && prop.GetValue(entity) == null && prop.CanWrite)
            prop.SetValue(entity, value);
    }

    /// <summary>将实体转为属性字典（排除导航属性/系统字段）</summary>
    internal static Dictionary<string, object?> EntityToDict<T>(T entity)
    {
        var dict = new Dictionary<string, object?>();
        ArgumentNullException.ThrowIfNull(entity);
        var props = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var p in props)
        {
            if (p.Name is "DomainEvents" or "RowVersion") continue;
            if (!p.CanWrite) continue;
            if (IsNavProp(p)) continue;
            dict[p.Name] = p.GetValue(entity);
        }
        return dict;
    }

    private static bool IsNavProp(System.Reflection.PropertyInfo p)
    {
        var t = p.PropertyType;
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;
        return t == typeof(System.Collections.IList) || t.IsGenericType
               || p.Name is "DomainEvents" or "RowVersion" or "Records" or "Roles";
    }
}
