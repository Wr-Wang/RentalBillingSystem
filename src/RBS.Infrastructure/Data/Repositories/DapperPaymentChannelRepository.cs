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

public class DapperPaymentChannelRepository : DapperRepository<PaymentChannel>, IPaymentChannelRepository
{
    private readonly ISqlLoader _sql;
    public DapperPaymentChannelRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, "PaymentChannels", tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<List<PaymentChannel>> GetActiveByCompanyAsync(Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<PaymentChannel>(_sql.Get("PaymentChannel.Select.PaymentChannel.Active"))).ToList(); }
}
