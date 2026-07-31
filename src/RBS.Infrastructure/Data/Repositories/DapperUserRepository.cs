using Dapper;
using System.Linq.Expressions;
using RBS.Core.Common;
using RBS.Core.Entities.Organization;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Infrastructure.Data.Services;

namespace RBS.Infrastructure.Data.Repositories;

public class DapperUserRepository : IUserRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly RepositoryAuditService _auditService;
    private readonly IChangeTracker? _tracker;

    public DapperUserRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, RepositoryAuditService? auditService = null)
    {
        _db = db; _sql = sql;
        _auditWriter = auditWriter;
        _auditService = auditService ?? new RepositoryAuditService(auditWriter);
        _tracker = tracker;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var entity = await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ById"), new { Id = id });
        if (entity != null) _tracker?.Track(entity, "Users");
        return entity;
    }
    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var list = (await conn.QueryAsync<User>(_sql.Get("Identity.Select.User.All"))).ToList();
        foreach (var e in list) _tracker?.Track(e, "Users");
        return list;
    }
    public async Task<User> AddAsync(User entity, CancellationToken ct = default)
    {
        _auditService.PopulateCreatedFields(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Insert.User.Default"), entity);
        await _auditService.WriteCreateLogAsync("Users", entity, ct);
        return entity;
    }
    public async Task UpdateAsync(User entity, CancellationToken ct = default)
    {
        _auditService.PopulateUpdatedFields(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Update.User.Default"), entity);
        await _auditService.WriteUpdateAsync("Users", entity, ct);
    }
    public async Task DeleteAsync(User entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Delete.User.ById"), new { entity.Id });
        var oldUser = await GetByIdAsync(entity.Id, ct);
        if (oldUser != null) await _auditService.WriteDeleteAsync("Users", oldUser, ct);
    }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>(_sql.Get("Identity.Select.User.Exists"), new { Id = id }) > 0;
    }
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ByUsername"), new { Username = username });
    }
    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ById"), new { Id = id });
    }
    public async Task<List<User>> GetAllWithRolesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(_sql.Get("Identity.Select.User.AllWithRoles"));
        var users = (await multi.ReadAsync<User>()).ToList();
        var roleRows = (await multi.ReadAsync<dynamic>()).ToList();
        var roleField = typeof(User).GetField("_roles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (roleField != null)
        {
            foreach (var u in users)
            {
                var urs = roleRows.Where(r => r.UserId == u.Id).Select(r => new UserRole(u.Id, (Guid)r.RoleId)).ToList();
                roleField.SetValue(u, urs);
            }
        }
        return users;
    }
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<string>(_sql.Get("Identity.Select.Permission.ByUserId"), new { UserId = userId })).ToList();
    }
    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        if (excludeId.HasValue)
            return await conn.QuerySingleAsync<int>(_sql.Get("Identity.Select.User.UsernameUniqueExclude"), new { Username = username, Id = excludeId }) == 0;
        return await conn.QuerySingleAsync<int>(_sql.Get("Identity.Select.User.UsernameUnique"), new { Username = username }) == 0;
    }
    public async Task ReplaceRolesAsync(Guid userId, List<Guid> newRoleIds, Guid changedBy, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(_sql.Get("Identity.Delete.UserRoles.ByUserId"), new { UserId = userId }, tx);
        foreach (var roleId in newRoleIds)
            await conn.ExecuteAsync(_sql.Get("Identity.Insert.UserRole.Default"),
                new { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, CreatedBy = changedBy, CreatedAt = ChinaTime.Now }, tx);
        tx.Commit();
        foreach (var roleId in newRoleIds)
        {
            var rowId = Guid.NewGuid();
            await _auditWriter.LogChangesAsync("UserRoles", rowId.ToString(), "Create",
                new Dictionary<string, object?>
                {
                    ["Id"] = rowId, ["UserId"] = userId, ["RoleId"] = roleId,
                    ["CreatedBy"] = changedBy, ["CreatedAt"] = ChinaTime.Now
                }, changedBy, ct);
        }
    }
    public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, Expression<Func<User, bool>>? predicate = null, Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}
