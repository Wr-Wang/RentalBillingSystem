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

public class DapperFeeCodeRepository : DapperRepository<FeeCode>, IFeeCodeRepository
{
    private readonly ISqlLoader _sql;
    public DapperFeeCodeRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, "FeeCodes", tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<FeeCode?> GetByCodeAsync(string code, Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<FeeCode>(_sql.Get("FeeCode.Select.FeeCode.ByCode"), new { Code = code }); }
    public async Task<List<FeeCode>> GetByCategoryAsync(string category, Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<FeeCode>(_sql.Get("FeeCode.Select.FeeCode.ByCategory"), new { Category = category })).ToList(); }
}
