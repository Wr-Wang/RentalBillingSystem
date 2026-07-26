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

public class DapperRoleRepository : IRoleRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly RepositoryAuditService _auditService;
    private readonly IChangeTracker? _tracker;

    public DapperRoleRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, RepositoryAuditService? auditService = null)
    {
        _db = db; _sql = sql;
        _auditService = auditService ?? new RepositoryAuditService(auditWriter);
        _tracker = tracker;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var e = await conn.QuerySingleOrDefaultAsync<Role>(_sql.Get("Authorization.Select.Role.ById"), new { Id = id });
        if (e != null) _tracker?.Track(e, "Roles"); return e;
    }
    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var list = (await conn.QueryAsync<Role>(_sql.Get("Authorization.Select.Role.All"))).ToList();
        foreach (var e in list) _tracker?.Track(e, "Roles"); return list;
    }
    public async Task<Role> AddAsync(Role entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Authorization.Insert.Role.Default"), entity);
        await _auditService.WriteCreateAsync("Roles", entity, ct);
        return entity;
    }
    public async Task UpdateAsync(Role entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Authorization.Update.Role.Default"), entity);
        await _auditService.WriteUpdateAsync("Roles", entity, ct);
    }
    public async Task DeleteAsync(Role entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Authorization.Delete.Role.ById"), new { entity.Id });
        var oldEntity = await GetByIdAsync(entity.Id, ct);
        if (oldEntity != null) await _auditService.WriteDeleteAsync("Roles", oldEntity, ct);
    }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>(_sql.Get("Authorization.Select.Role.Exists"), new { Id = id }) > 0;
    }
    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<Role>(_sql.Get("Authorization.Select.Role.ByCode"), new { Code = code });
    }
    public async Task<List<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<Role>(_sql.Get("Authorization.Select.Role.ByUserId"), new { UserId = userId })).ToList();
    }
    public async Task<Role?> GetByIdWithRoleMenusAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(_sql.Get("Authorization.Select.Role.WithMenus"), new { Id = id });
        var role = await multi.ReadSingleOrDefaultAsync<Role>();
        if (role != null)
        {
            var menuIds = (await multi.ReadAsync<Guid>()).ToList();
            var field = typeof(Role).GetField("_roleMenus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var menus = menuIds.Select(mid => new RoleMenu(role.Id, mid)).ToList();
                field.SetValue(role, menus);
            }
        }
        return role;
    }
    public async Task SaveRoleMenusAsync(Guid roleId, List<Guid> menuIds, Guid changedBy, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(_sql.Get("Authorization.Delete.RoleMenus.ByRoleId"), new { RoleId = roleId }, tx);
        foreach (var menuId in menuIds)
            await conn.ExecuteAsync(_sql.Get("Authorization.Insert.RoleMenu.Default"),
                new { Id = Guid.NewGuid(), RoleId = roleId, MenuId = menuId, CreatedBy = changedBy, CreatedAt = ChinaTime.Now }, tx);
        tx.Commit();
    }
    public Task<PagedResult<Role>> GetPagedAsync(int page, int pageSize, Expression<Func<Role, bool>>? predicate = null, Func<IQueryable<Role>, IOrderedQueryable<Role>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}
