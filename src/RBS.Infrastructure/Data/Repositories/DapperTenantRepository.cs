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

public class DapperTenantRepository : DapperRepository<Tenant>, ITenantRepository
{
    private readonly ISqlLoader _sql;
    public DapperTenantRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, tracker: tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<Tenant?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Tenant>(_sql.Get("Rental.Select.Tenant.ByPhone"), new { Phone = phone }); }
    public async Task<List<Tenant>> SearchAsync(string keyword, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Tenant>(_sql.Get("Rental.Select.Tenant.Search"), new { K = $"%{keyword}%" })).ToList(); }
}
