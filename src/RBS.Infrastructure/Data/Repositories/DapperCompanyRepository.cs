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

public class DapperCompanyRepository : ICompanyRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IChangeTracker? _tracker;
    public DapperCompanyRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null) { _db = db; _sql = sql; _auditWriter = auditWriter; _tracker = tracker; }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var e = await conn.QuerySingleOrDefaultAsync<Company>(_sql.Get("Organization.Select.Company.ById"), new { Id = id }); if (e != null) _tracker?.Track(e, "Companies"); return e; }
    public async Task<List<Company>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var list = (await conn.QueryAsync<Company>(_sql.Get("Organization.Select.Company.All"))).ToList(); foreach (var e in list) _tracker?.Track(e, "Companies"); return list; }
    public async Task<Company> AddAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Organization.Insert.Company.Default"), entity); await _auditWriter.LogChangesAsync("Companies", entity.Id.ToString(), "Create", new() { ["Name"] = entity.Name, ["Code"] = entity.Code }, entity.CreatedBy, ct); return entity; }
    public async Task UpdateAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Organization.Update.Company.Default"), entity); await _auditWriter.LogChangesAsync("Companies", entity.Id.ToString(), "Update", new() { ["Name"] = entity.Name }, entity.UpdatedBy ?? Guid.Empty, ct); }
    public async Task DeleteAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Organization.Delete.Company.ById"), new { entity.Id }); await _auditWriter.LogChangesAsync("Companies", entity.Id.ToString(), "Delete", new() { ["Id"] = entity.Id.ToString() }, Guid.Empty, ct); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Organization.Select.Company.Exists"), new { Id = id }) > 0; }
    public async Task<Company?> GetByNameAsync(string name, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Company>(_sql.Get("Organization.Select.Company.ByName"), new { Name = name }); }
    public async Task<List<Company>> GetActiveAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Company>(_sql.Get("Organization.Select.Company.Active"))).ToList(); }
    public Task<PagedResult<Company>> GetPagedAsync(int page, int pageSize, Expression<Func<Company, bool>>? predicate = null, Func<IQueryable<Company>, IOrderedQueryable<Company>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}
