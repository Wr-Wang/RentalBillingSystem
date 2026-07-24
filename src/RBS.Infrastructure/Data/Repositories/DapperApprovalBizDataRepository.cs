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

public class DapperApprovalBizDataRepository : DapperRepository<ApprovalBizData>, IApprovalBizDataRepository
{
    private readonly ISqlLoader _sql;
    public DapperApprovalBizDataRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, "ApprovalBizData", tracker, tenant: tenant)
    {
        _sql = sql;
    }

    public async Task<ApprovalBizData?> GetByApprovalRequestIdAsync(Guid approvalRequestId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); var e = await conn.QuerySingleOrDefaultAsync<ApprovalBizData>(_sql.Get("Approval.Select.ApprovalBizData.ByApprovalRequestId"), new { Id = approvalRequestId }); if (e != null) _tracker?.Track(e, "ApprovalBizData"); return e; }

    public async Task<List<ApprovalBizData>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); var list = (await conn.QueryAsync<ApprovalBizData>(_sql.Get("Approval.Select.ApprovalBizData.ByContractId"), new { Id = contractId })).ToList(); foreach (var e in list) _tracker?.Track(e, "ApprovalBizData"); return list; }
}
