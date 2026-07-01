using Dapper;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class DapperRepository<T> : IRepository<T> where T : RBS.Core.Entities.Base.AuditableEntity
{
    protected readonly IDbConnectionFactory _db;
    protected readonly string _tableName;

    public DapperRepository(IDbConnectionFactory db, string? tableName = null)
    {
        _db = db;
        _tableName = tableName ?? InferTableName();
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<T>($"SELECT * FROM [{_tableName}] WHERE Id=@Id", new { Id = id });
    }

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<T>($"SELECT * FROM [{_tableName}]")).ToList();
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && !IsNavProp(p))
            .Select(p => p.Name).ToList();
        var cols = string.Join(",", props);
        var vals = string.Join(",", props.Select(p => "@" + p));
        await conn.ExecuteAsync($"INSERT INTO [{_tableName}] ({cols}) VALUES ({vals})", entity);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var exclude = new HashSet<string> { "Id", "CreatedBy", "CreatedAt", "CreatedIp", "CreatedHostname" };
        var sets = string.Join(",",
            typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanRead && !exclude.Contains(p.Name) && !IsNavProp(p))
                .Select(p => $"[{p.Name}]=@{p.Name}"));
        await conn.ExecuteAsync($"UPDATE [{_tableName}] SET {sets} WHERE Id=@Id", entity);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        var id = typeof(T).GetProperty("Id")?.GetValue(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync($"DELETE FROM [{_tableName}] WHERE Id=@Id", new { Id = id });
    }

    public Task<PagedResult<T>> GetPagedAsync(int page, int pageSize,
        System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken ct = default) => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>($"SELECT COUNT(1) FROM [{_tableName}] WHERE Id=@Id", new { Id = id }) > 0;
    }

    private static bool IsNavProp(System.Reflection.PropertyInfo p)
    {
        var t = p.PropertyType;
        return t == typeof(System.Collections.IList) || t.IsGenericType ||
               p.Name is "DomainEvents" or "RowVersion" or "Records" or "Roles";
    }

    private static string InferTableName()
    {
        var name = typeof(T).Name;
        if (name.EndsWith("y")) return name.Substring(0, name.Length - 1) + "ies";
        if (name.EndsWith("s")) return name + "es";
        return name + "s";
    }
}
