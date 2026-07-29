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

public class DapperMeterReadingRepository : DapperRepository<MeterReading>, IMeterReadingRepository
{
    private readonly ISqlLoader _sql;
    public DapperMeterReadingRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, tracker: tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<MeterReading?> GetLatestReadingAsync(Guid contractFeeConfigId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<MeterReading>(_sql.Get("Utility.Select.MeterReading.Latest"), new { Id = contractFeeConfigId }); }
    public async Task<List<MeterReading>> GetHistoryAsync(Guid contractFeeConfigId, int year, int month, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<MeterReading>(_sql.Get("Utility.Select.MeterReading.History"), new { Id = contractFeeConfigId, Y = year, M = month })).ToList(); }
    public async Task<bool> ReadingExistsAsync(Guid contractFeeConfigId, int year, int month, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Utility.Select.MeterReading.Exists"), new { Id = contractFeeConfigId, Y = year, M = month }) > 0; }
    public async Task ConfirmAsync(Guid id, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Utility.Update.MeterReading.Confirm"), new { Id = id }); }
    public async Task ConfirmWithReadingAsync(Guid id, decimal currentReading, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Utility.Update.MeterReading.ConfirmWithReading"), new { Id = id, Current = currentReading }); }
    public async Task<IEnumerable<dynamic>> GetByCompanyMonthAsync(Guid companyId, int year, int month, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QueryAsync(_sql.Get("Utility.Select.MeterReading.ByCompanyMonth"), new { CompanyId = companyId, Year = year, Month = month }); }
}
