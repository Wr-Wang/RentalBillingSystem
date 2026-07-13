using Dapper;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Repositories;

public class DapperRepository<T> : IRepository<T> where T : AuditableEntity
{
    protected readonly IDbConnectionFactory _db;
    protected readonly string _tableName;
    protected readonly IAuditLogWriter _auditWriter;
    protected readonly IChangeTracker? _tracker;
    protected readonly ITenantService? _tenant;

    public DapperRepository(IDbConnectionFactory db, IAuditLogWriter auditWriter, string? tableName = null, IChangeTracker? tracker = null, ITenantService? tenant = null)
    {
        _db = db;
        _auditWriter = auditWriter;
        _tracker = tracker;
        _tableName = tableName ?? InferTableName();
        _tenant = tenant;
    }

    private bool HasCompanyFilter([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Guid companyId)
    {
        companyId = Guid.Empty;
        if (_tenant == null) return false;
        var cid = _tenant.EffectiveCompanyId;
        if (cid == null) return false;
        if (!typeof(IHasCompany).IsAssignableFrom(typeof(T))) return false;
        companyId = cid.Value;
        return true;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        T? entity;
        if (HasCompanyFilter(out var cid))
            entity = await conn.QuerySingleOrDefaultAsync<T>($"SELECT * FROM [{_tableName}] WHERE Id=@Id AND CompanyId=@CompanyId", new { Id = id, CompanyId = cid });
        else
            entity = await conn.QuerySingleOrDefaultAsync<T>($"SELECT * FROM [{_tableName}] WHERE Id=@Id", new { Id = id });
        if (entity != null) _tracker?.Track(entity, _tableName);
        return entity;
    }

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        List<T> list;
        if (HasCompanyFilter(out var cid))
            list = (await conn.QueryAsync<T>($"SELECT * FROM [{_tableName}] WHERE CompanyId=@CompanyId", new { CompanyId = cid })).ToList();
        else
            list = (await conn.QueryAsync<T>($"SELECT * FROM [{_tableName}]")).ToList();
        foreach (var entity in list) _tracker?.Track(entity, _tableName);
        return list;
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var exclude = new HashSet<string> { "CreatedIp", "CreatedHostname", "UpdatedBy", "UpdatedAt", "UpdatedIp", "UpdatedHostname" };
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && !exclude.Contains(p.Name) && !IsNavProp(p))
            .Select(p => p.Name).ToList();
        var cols = string.Join(",", props);
        var vals = string.Join(",", props.Select(p => "@" + p));
        await conn.ExecuteAsync($"INSERT INTO [{_tableName}] ({cols}) VALUES ({vals})", entity);

        // 审计：记录所有字段
        var createdBy = typeof(T).GetProperty("CreatedBy")?.GetValue(entity) as Guid? ?? Guid.Empty;
        await _auditWriter.LogChangesAsync(_tableName, GetEntityId(entity), "Create", EntityToDict(entity), createdBy, ct);
        _tracker?.Track(entity, _tableName);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // 1. 读取旧值
        var oldEntity = await GetByIdAsync(GetEntityGuidId(entity), ct);

        // 2. 更新主表
        using var conn = _db.CreateConnection(); conn.Open();
        var exclude = new HashSet<string> { "Id", "CreatedBy", "CreatedAt", "CreatedIp", "CreatedHostname", "UpdatedBy", "UpdatedAt", "UpdatedIp", "UpdatedHostname" };
        var sets = string.Join(",",
            typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && !exclude.Contains(p.Name) && !IsNavProp(p))
                .Select(p => $"[{p.Name}]=@{p.Name}"));
        await conn.ExecuteAsync($"UPDATE [{_tableName}] SET {sets} WHERE Id=@Id", entity);

        // 3. 审计：计算差异
        if (oldEntity != null)
        {
            var changes = DiffDict(EntityToDict(oldEntity), EntityToDict(entity));
            var updatedBy = typeof(T).GetProperty("UpdatedBy")?.GetValue(entity) as Guid? ?? Guid.Empty;
            await _auditWriter.LogChangesAsync(_tableName, GetEntityId(entity), "Update", changes, updatedBy, ct);
        }
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        var id = GetEntityGuidId(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync($"DELETE FROM [{_tableName}] WHERE Id=@Id", new { Id = id });

        // 审计
        var changes = new Dictionary<string, object?> { ["Id"] = id.ToString() };
        await _auditWriter.LogChangesAsync(_tableName, id.ToString(), "Delete", changes, Guid.Empty, ct);
    }

    public Task<PagedResult<T>> GetPagedAsync(int page, int pageSize,
        System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken ct = default) => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        if (HasCompanyFilter(out var cid))
            return await conn.QuerySingleAsync<int>($"SELECT COUNT(1) FROM [{_tableName}] WHERE Id=@Id AND CompanyId=@CompanyId", new { Id = id, CompanyId = cid }) > 0;
        return await conn.QuerySingleAsync<int>($"SELECT COUNT(1) FROM [{_tableName}] WHERE Id=@Id", new { Id = id }) > 0;
    }

    // ===== 辅助方法 =====

    private static string GetEntityId(T entity)
    {
        return typeof(T).GetProperty("Id")?.GetValue(entity)?.ToString() ?? Guid.Empty.ToString();
    }

    private static Guid GetEntityGuidId(T entity)
    {
        return typeof(T).GetProperty("Id")?.GetValue(entity) is Guid g ? g : Guid.Empty;
    }

    protected static Dictionary<string, object?> EntityToDict(T entity)
    {
        var dict = new Dictionary<string, object?>();
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var p in props)
        {
            if (IsNavProp(p) || p.Name is "DomainEvents" or "RowVersion") continue;
            if (!p.CanWrite) continue; // 排除计算属性（如 IsVacant/IsRented）
            dict[p.Name] = p.GetValue(entity);
        }
        return dict;
    }

    protected static Dictionary<string, object?> DiffDict(Dictionary<string, object?> old, Dictionary<string, object?> now)
    {
        var diff = new Dictionary<string, object?>();
        var exclude = new HashSet<string> { "RowVersion", "UpdatedAt", "UpdatedBy", "UpdatedIp", "UpdatedHostname" };
        foreach (var kv in now)
        {
            if (exclude.Contains(kv.Key)) continue;
            if (!old.ContainsKey(kv.Key)) { diff[kv.Key] = kv.Value; continue; }
            var oldVal = old[kv.Key];
            var newVal = kv.Value;
            if (!Equals(oldVal, newVal))
                diff[kv.Key] = newVal;
        }
        return diff;
    }

    private static bool IsNavProp(System.Reflection.PropertyInfo p)
    {
        var t = p.PropertyType;
        // 可空值类型 Nullable<T> 不是导航属性，应参与 INSERT/UPDATE
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;
        return t == typeof(System.Collections.IList) || t.IsGenericType ||
               p.Name is "DomainEvents" or "RowVersion" or "Records" or "Roles";
    }

    private static string InferTableName()
    {
        var name = typeof(T).Name;
        if (name.EndsWith("y")) return name.Substring(0, name.Length - 1) + "ies";
        if (name.EndsWith("s") || name.EndsWith("ch") || name.EndsWith("sh") || name.EndsWith("x")) return name + "es";
        return name + "s";
    }
}
