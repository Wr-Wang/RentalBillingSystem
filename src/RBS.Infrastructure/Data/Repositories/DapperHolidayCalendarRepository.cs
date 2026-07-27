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

public class DapperHolidayCalendarRepository : DapperRepository<HolidayCalendar>, IHolidayCalendarRepository
{
    private readonly ISqlLoader _sql;
    public DapperHolidayCalendarRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, "HolidayCalendars", tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<List<HolidayCalendar>> GetByYearAsync(Guid companyId, int year, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<HolidayCalendar>(_sql.Get("Calendar.Select.Holiday.ByYear"), new { Year = year })).ToList(); }
    public async Task<HolidayCalendar?> GetByDateAsync(Guid companyId, DateTime date, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<HolidayCalendar>(_sql.Get("Calendar.Select.Holiday.ByDate"), new { Date = date }); }
}
