using Dapper;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Infrastructure.Data.Services;

namespace RBS.Infrastructure.Data.Repositories;

/// <summary>
/// Dapper 泛型仓储基类 — 实现 IRepository&lt;T&gt; 接口，提供基于反射的 CRUD 操作。
/// 审计逻辑统一委托给 RepositoryAuditService（审计装饰器）。
/// </summary>
/// <typeparam name="T">实体类型，必须继承自 AuditableEntity</typeparam>
public class DapperRepository<T> : IRepository<T> where T : AuditableEntity
{
    protected readonly IDbConnectionFactory _db;
    protected readonly string _tableName;
    /// <summary>审计日志写入器（已废弃，请使用 _auditService）</summary>
    protected readonly IAuditLogWriter _auditWriter;
    protected readonly IChangeTracker? _tracker;
    protected readonly ITenantService? _tenant;
    /// <summary>审计装饰器 — 统一处理所有审计逻辑</summary>
    protected readonly RepositoryAuditService _auditService;

    public DapperRepository(IDbConnectionFactory db, IAuditLogWriter auditWriter,
        string? tableName = null, IChangeTracker? tracker = null,
        ITenantService? tenant = null, RepositoryAuditService? auditService = null)
    {
        _db = db;
        _auditWriter = auditWriter;
        _auditService = auditService ?? new RepositoryAuditService(auditWriter);
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
        // ★ 审计装饰器：INSERT 前填充 CreatedBy/CreatedIp/CreatedHostname，确保入库
        _auditService.PopulateCreatedFields(entity);

        using var conn = _db.CreateConnection(); conn.Open();
        var exclude = new HashSet<string> { "UpdatedBy", "UpdatedAt", "UpdatedIp", "UpdatedHostname" };
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && !exclude.Contains(p.Name) && !IsNavProp(p))
            .Select(p => p.Name).ToList();
        var cols = string.Join(",", props);
        var vals = string.Join(",", props.Select(p => "@" + p));
        await conn.ExecuteAsync($"INSERT INTO [{_tableName}] ({cols}) VALUES ({vals})", entity);

        // ★ 审计装饰器统一处理：INSERT 后写审计日志
        await _auditService.WriteCreateLogAsync(_tableName, entity, ct);
        _tracker?.Track(entity, _tableName);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        var oldEntity = await GetByIdAsync(GetEntityGuidId(entity), ct);

        // 在 UPDATE 前填充 UpdatedBy/At/Ip/Hostname，使业务表也能记录操作人
        _auditService.PopulateUpdatedFields(entity);

        using var conn = _db.CreateConnection(); conn.Open();
        var exclude = new HashSet<string> { "Id", "CreatedBy", "CreatedAt", "CreatedIp", "CreatedHostname" };
        var sets = string.Join(",",
            typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && !exclude.Contains(p.Name) && !IsNavProp(p))
                .Select(p => $"[{p.Name}]=@{p.Name}"));
        await conn.ExecuteAsync($"UPDATE [{_tableName}] SET {sets} WHERE Id=@Id", entity);

        // ★ 审计装饰器统一处理
        if (oldEntity != null)
        {
            await _auditService.WriteUpdateAsync(_tableName, entity, ct);
        }
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        var oldEntity = await GetByIdAsync(GetEntityGuidId(entity), ct);
        var id = GetEntityGuidId(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync($"DELETE FROM [{_tableName}] WHERE Id=@Id", new { Id = id });

        if (oldEntity != null)
        {
            // ★ 审计装饰器统一处理（传 oldEntity 记录删除前的快照）
            await _auditService.WriteDeleteAsync(_tableName, oldEntity, ct);
        }
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

    private static string GetEntityId(T entity)
        => typeof(T).GetProperty("Id")?.GetValue(entity)?.ToString() ?? Guid.Empty.ToString();

    private static Guid GetEntityGuidId(T entity)
        => typeof(T).GetProperty("Id")?.GetValue(entity) is Guid g ? g : Guid.Empty;

    protected static Dictionary<string, object?> EntityToDict(T entity)
        => RepositoryAuditService.EntityToDict(entity);

    private static bool IsNavProp(System.Reflection.PropertyInfo p)
    {
        var t = p.PropertyType;
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;
        return t == typeof(System.Collections.IList) || t.IsGenericType ||
               p.Name is "DomainEvents" or "Records" or "Roles";
    }

    private static string InferTableName()
    {
        var name = typeof(T).Name;
        if (name.EndsWith("y")) return name.Substring(0, name.Length - 1) + "ies";
        if (name.EndsWith("s") || name.EndsWith("ch") || name.EndsWith("sh") || name.EndsWith("x")) return name + "es";
        return name + "s";
    }
}
