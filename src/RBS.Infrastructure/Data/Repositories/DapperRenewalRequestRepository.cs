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

public class DapperRenewalRequestRepository : DapperRepository<RenewalRequest>, IRenewalRequestRepository
{
    private readonly ISqlLoader _sql;
    public DapperRenewalRequestRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, tracker: tracker, tenant: tenant)
    {
        _sql = sql;
    }

    public async Task<List<RenewalRequest>> GetByOldContractIdAsync(Guid oldContractId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<RenewalRequest>(_sql.Get("Lease.Select.RenewalRequest.ByOldContractId"), new { Id = oldContractId })).ToList(); }

    public async Task<RenewalRequest?> GetPendingByContractIdAsync(Guid contractId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<RenewalRequest>(_sql.Get("Lease.Select.RenewalRequest.PendingByContractId"), new { Id = contractId }); }

    public async Task<bool> HasPendingForContractAsync(Guid contractId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Lease.Select.RenewalRequest.HasPendingForContract"), new { Id = contractId }) > 0; }
}
