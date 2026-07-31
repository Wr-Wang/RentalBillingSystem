using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计装饰器 — 统一封装所有仓储的审计写入逻辑
/// 各仓储不再直接调用 IAuditLogWriter，全部委托给此类
///
/// 用户 ID 获取优先级：
///   1. 实体上的 UpdatedBy（非空时）
///   2. 实体上的 CreatedBy（非空时）
///   3. ICurrentUserService.UserId（从 JWT token 解析）
///   4. Guid.Empty（兜底，仅后台任务等无用户上下文场景）
/// </summary>
public class RepositoryAuditService
{
    private readonly IAuditLogWriter _auditWriter;
    private readonly IClientInfoService? _clientInfo;
    private readonly ICurrentUserService? _currentUser;

    public RepositoryAuditService(IAuditLogWriter auditWriter, IClientInfoService? clientInfo = null, ICurrentUserService? currentUser = null)
    {
        _auditWriter = auditWriter;
        _clientInfo = clientInfo;
        _currentUser = currentUser;
    }

    /// <summary>写入创建审计 + 填充 CreatedIp/CreatedHostname/CreatedBy</summary>
    public async Task WriteCreateAsync<T>(string tableName, T entity, CancellationToken ct = default)
    {
        // 从请求上下文填充 CreatedIp/CreatedHostname
        if (_clientInfo != null)
        {
            TrySetProperty(entity, "CreatedIp", _clientInfo.GetClientIp());
            TrySetProperty(entity, "CreatedHostname", _clientInfo.GetClientHostname());
        }
        // 如果服务层传了 Guid.Empty，用当前登录用户填充
        EnsureCreatedBy(entity);

        var dict = EntityToDict(entity);
        var createdBy = ResolveUserId(entity, useUpdated: false);
        await _auditWriter.LogChangesAsync(tableName, GetEntityId(entity), "Create", dict, createdBy, ct);
    }

    /// <summary>实体 CreatedBy 为 Guid.Empty 时，用当前登录用户填充</summary>
    private void EnsureCreatedBy<T>(T entity)
    {
        if (_currentUser == null || _currentUser.UserId == Guid.Empty) return;
        var prop = typeof(T).GetProperty("CreatedBy");
        if (prop == null || !prop.CanWrite) return;
        if (prop.GetValue(entity) is Guid g && g == Guid.Empty)
            prop.SetValue(entity, _currentUser.UserId);
    }

    /// <summary>写入更新审计 + 填充 UpdatedIp/UpdatedHostname</summary>
    public async Task WriteUpdateAsync<T>(string tableName, T entity, CancellationToken ct = default)
    {
        var dict = EntityToDict(entity);
        if (_clientInfo != null)
        {
            dict["UpdatedIp"] = _clientInfo.GetClientIp();
            dict["UpdatedHostname"] = _clientInfo.GetClientHostname();
        }
        var updatedBy = ResolveUserId(entity, useUpdated: true);
        await _auditWriter.LogChangesAsync(tableName, GetEntityId(entity), "Update", dict, updatedBy, ct);
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
        var deletedBy = ResolveUserId(entity, useUpdated: true);
        await _auditWriter.LogChangesAsync(tableName, GetEntityId(entity), "Delete", dict, deletedBy, ct);
    }

    /// <summary>获取客户端 IP/主机名（供 UoW CommitAsync 使用）</summary>
    public (string? ip, string? hostname) GetClientInfo()
        => (_clientInfo?.GetClientIp(), _clientInfo?.GetClientHostname());

    /// <summary>
    /// 在 UPDATE 前设置实体的审计字段（UpdatedBy/At/Ip/Hostname），
    /// 使业务表的 Updated* 列也能记录当前操作人
    /// </summary>
    public void PopulateUpdatedFields<T>(T entity)
    {
        var now = RBS.Core.Common.ChinaTime.Now;
        // UpdatedBy
        var ub = typeof(T).GetProperty("UpdatedBy");
        if (ub != null && ub.CanWrite)
        {
            var uid = _currentUser?.UserId ?? Guid.Empty;
            if (uid != Guid.Empty) ub.SetValue(entity, uid);
        }
        // UpdatedAt
        var ua = typeof(T).GetProperty("UpdatedAt");
        if (ua != null && ua.CanWrite) ua.SetValue(entity, now);
        // UpdatedIp / UpdatedHostname
        if (_clientInfo != null)
        {
            TrySetProperty(entity, "UpdatedIp", _clientInfo.GetClientIp());
            TrySetProperty(entity, "UpdatedHostname", _clientInfo.GetClientHostname());
        }
    }

    // ===== 内部工具方法 =====

    /// <summary>
    /// 解析操作人 ID，优先级：
    /// 实体.UpdatedBy → ICurrentUserService → 实体.CreatedBy → Guid.Empty
    /// </summary>
    private Guid ResolveUserId<T>(T entity, bool useUpdated)
    {
        if (useUpdated)
        {
            var ub = TryGetGuid(entity!, "UpdatedBy");
            if (ub.GetValueOrDefault() != Guid.Empty) return ub!.Value;
        }
        // 当前登录用户（从 JWT）优先级高于实体的 CreatedBy
        if (_currentUser != null && _currentUser.UserId != Guid.Empty)
            return _currentUser.UserId;
        var cb = TryGetGuid(entity!, "CreatedBy");
        if (cb.GetValueOrDefault() != Guid.Empty) return cb!.Value;
        return Guid.Empty;
    }

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
            if (p.Name is "DomainEvents") continue;
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
               || p.Name is "DomainEvents" or "Records" or "Roles";
    }
}
