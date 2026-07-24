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

public class DapperReceiptRepository : DapperRepository<Receipt>, IReceiptRepository
{
    private readonly ISqlLoader _sql;
    public DapperReceiptRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, tracker: tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<List<Receipt>> GetPendingConfirmAsync(Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Receipt>(_sql.Get("Collection.Select.Receipt.PendingConfirm"), new { Id = companyId })).ToList(); }
    public async Task<List<Receipt>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Receipt>(_sql.Get("Collection.Select.Receipt.All"), new { Id = companyId })).ToList(); }
    public async Task<decimal> GetTotalConfirmedAsync(Guid contractId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<decimal>(_sql.Get("Collection.Select.Receipt.TotalConfirmed"), new { Id = contractId }); }
}
