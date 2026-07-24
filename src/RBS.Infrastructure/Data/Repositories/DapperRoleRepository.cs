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

namespace RBS.Infrastructure.Data.Repositories;

public class DapperRoleRepository : IRoleRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IChangeTracker? _tracker;
    public DapperRoleRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null) { _db = db; _sql = sql; _auditWriter = auditWriter; _tracker = tracker; }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var e = await conn.QuerySingleOrDefaultAsync<Role>(_sql.Get("Authorization.Select.Role.ById"), new { Id = id }); if (e != null) _tracker?.Track(e, "Roles"); return e; }
    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var list = (await conn.QueryAsync<Role>(_sql.Get("Authorization.Select.Role.All"))).ToList(); foreach (var e in list) _tracker?.Track(e, "Roles"); return list; }
    public async Task<Role> AddAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Insert.Role.Default"), entity); await _auditWriter.LogChangesAsync("Roles", entity.Id.ToString(), "Create", new() { ["Name"] = entity.Name, ["Code"] = entity.Code }, entity.CreatedBy, ct); return entity; }
    public async Task UpdateAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Update.Role.Default"), entity); await _auditWriter.LogChangesAsync("Roles", entity.Id.ToString(), "Update", new() { ["Name"] = entity.Name }, entity.UpdatedBy ?? Guid.Empty, ct); }
    public async Task DeleteAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Delete.Role.ById"), new { entity.Id }); await _auditWriter.LogChangesAsync("Roles", entity.Id.ToString(), "Delete", new() { ["Id"] = entity.Id.ToString() }, Guid.Empty, ct); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Authorization.Select.Role.Exists"), new { Id = id }) > 0; }
    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Role>(_sql.Get("Authorization.Select.Role.ByCode"), new { Code = code }); }
    public async Task<List<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Role>(_sql.Get("Authorization.Select.Role.ByUserId"), new { UserId = userId })).ToList(); }
    /// <summary>
    /// 查询角色及其关联的菜单 ID 列表
    /// </summary>
    /// <remarks>
    /// 使用 QueryMultipleAsync 一次性加载角色信息和关联菜单 ID，
    /// 通过反射将 RoleMenu 列表写入角色的私有 _roleMenus 字段。
    /// </remarks>
    /// <param name="id">角色 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>含菜单关联的角色，未找到时返回 null</returns>
    public async Task<Role?> GetByIdWithRoleMenusAsync(Guid id, CancellationToken ct = default) {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Authorization.Select.Role.WithMenus"), new { Id = id });
        var role = await multi.ReadSingleOrDefaultAsync<Role>();
        if (role != null)
        {
            var menuIds = (await multi.ReadAsync<Guid>()).ToList();
            var field = typeof(Role).GetField("_roleMenus",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var menus = menuIds.Select(mid => new RoleMenu(role.Id, mid)).ToList();
                field.SetValue(role, menus);
            }
        }
        return role;
    }

    /// <summary>
    /// 保存角色的菜单权限（全量覆盖：先删后插）。
    /// 变更追踪无法感知 _roleMenus 集合变化，需直接操作 RoleMenus 表。
    /// </summary>
    public async Task SaveRoleMenusAsync(Guid roleId, List<Guid> menuIds, Guid changedBy, CancellationToken ct = default) {
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
