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

public class DapperMenuRepository : IMenuRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly RepositoryAuditService _auditService;
    private readonly IChangeTracker? _tracker;

    public DapperMenuRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, RepositoryAuditService? auditService = null)
    {
        _db = db; _sql = sql;
        _auditService = auditService ?? new RepositoryAuditService(auditWriter);
        _tracker = tracker;
    }

    public async Task<Menu?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var e = await conn.QuerySingleOrDefaultAsync<Menu>(_sql.Get("Authorization.Select.Menu.ById"), new { Id = id });
        if (e != null) _tracker?.Track(e, "Menus"); return e;
    }
    public async Task<List<Menu>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var list = (await conn.QueryAsync<Menu>(_sql.Get("Authorization.Select.Menu.All"))).ToList();
        foreach (var e in list) _tracker?.Track(e, "Menus"); return list;
    }
    public async Task<Menu> AddAsync(Menu entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Authorization.Insert.Menu.Default"), entity);
        await _auditService.WriteCreateAsync("Menus", entity, ct);
        return entity;
    }
    public async Task UpdateAsync(Menu entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Authorization.Update.Menu.Default"), entity);
        await _auditService.WriteUpdateAsync("Menus", entity, ct);
    }
    public async Task DeleteAsync(Menu entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Authorization.Delete.Menu.ById"), new { entity.Id });
        var oldEntity = await GetByIdAsync(entity.Id, ct);
        if (oldEntity != null) await _auditService.WriteDeleteAsync("Menus", oldEntity, ct);
    }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>(_sql.Get("Authorization.Select.Menu.Exists"), new { Id = id }) > 0;
    }
    public async Task<List<Menu>> GetByRoleIdsAsync(List<Guid> roleIds, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<Menu>(_sql.Get("Authorization.Select.Menu.ByRoleIds"), new { Ids = roleIds })).ToList();
    }
    public async Task<List<Menu>> GetTreeAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<Menu>(_sql.Get("Authorization.Select.Menu.All"))).ToList();
    }
    public Task<PagedResult<Menu>> GetPagedAsync(int page, int pageSize, Expression<Func<Menu, bool>>? predicate = null, Func<IQueryable<Menu>, IOrderedQueryable<Menu>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}
