using Dapper;
using RBS.Core.Interfaces.Persistence;
using System.Data;

namespace RBS.Application.Common;

public static class DapperHelper
{
    public static IDbConnection Open(this IDbConnectionFactory db)
    {
        var conn = db.CreateConnection();
        conn.Open();
        return conn;
    }

    public static async Task<List<T>> GetAllAsync<T>(this IDbConnectionFactory db, string table) where T : class
    {
        using var conn = db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<T>($"SELECT * FROM [{table}]")).ToList();
    }

    public static async Task<T?> GetByIdAsync<T>(this IDbConnectionFactory db, string table, Guid id) where T : class
    {
        using var conn = db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<T>($"SELECT * FROM [{table}] WHERE Id=@Id", new { Id = id });
    }

    public static async Task<bool> ExistsAsync(this IDbConnectionFactory db, string table, Guid id)
    {
        using var conn = db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>($"SELECT COUNT(1) FROM [{table}] WHERE Id=@Id", new { Id = id }) > 0;
    }

    public static async Task DeleteByIdAsync(this IDbConnectionFactory db, string table, Guid id)
    {
        using var conn = db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync($"DELETE FROM [{table}] WHERE Id=@Id", new { Id = id });
    }

    public static async Task InsertAsync<T>(this IDbConnectionFactory db, string table, T entity) where T : class
    {
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && p.Name != "DomainEvents" && p.Name != "RowVersion" && p.Name != "Records" && p.Name != "Roles"
                && !p.PropertyType.IsGenericType && p.PropertyType != typeof(System.Collections.IList))
            .Select(p => p.Name).ToList();
        var cols = string.Join(",", props);
        var vals = string.Join(",", props.Select(p => "@" + p));
        using var conn = db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync($"INSERT INTO [{table}] ({cols}) VALUES ({vals})", entity);
    }

    public static async Task UpdateAsync<T>(this IDbConnectionFactory db, string table, T entity) where T : class
    {
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && p.Name != "Id" && p.Name != "CreatedBy" && p.Name != "CreatedAt" && p.Name != "CreatedIp" && p.Name != "CreatedHostname"
                && p.Name != "DomainEvents" && p.Name != "RowVersion" && p.Name != "Records" && p.Name != "Roles"
                && !p.PropertyType.IsGenericType && p.PropertyType != typeof(System.Collections.IList))
            .Select(p => $"[{p.Name}]=@{p.Name}");
        using var conn = db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync($"UPDATE [{table}] SET {string.Join(",", props)} WHERE Id=@Id", entity);
    }
}
