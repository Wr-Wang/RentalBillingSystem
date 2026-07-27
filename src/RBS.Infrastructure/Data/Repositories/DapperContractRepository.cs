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

public class DapperContractRepository : DapperRepository<Contract>, IContractRepository
{
    private readonly ISqlLoader _sql;
    public DapperContractRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, tracker: tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<Contract?> GetByContractNoAsync(string contractNo, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Contract>(_sql.Get("Lease.Select.Contract.ByContractNo"), new { No = contractNo }); }
    public async Task<List<Contract>> GetActiveContractsAsync(Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Contract>(_sql.Get("Lease.Select.Contract.Active"), new { Id = companyId })).ToList(); }
    public async Task<List<Contract>> GetContractsExpiringAsync(DateTime date, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Contract>(_sql.Get("Lease.Select.Contract.Expiring"), new { Date = date })).ToList(); }
    public async Task<bool> HasActiveForHousingUnitAsync(Guid housingUnitId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Lease.Select.Contract.HasActiveForHousingUnit"), new { Id = housingUnitId }) > 0; }
}
