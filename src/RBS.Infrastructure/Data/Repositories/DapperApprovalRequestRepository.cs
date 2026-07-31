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
using RBS.Infrastructure.Data.Services;

namespace RBS.Infrastructure.Data.Repositories;

public class DapperApprovalRequestRepository : IApprovalRequestRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly RepositoryAuditService _auditService;
    private readonly IChangeTracker? _tracker;

    public DapperApprovalRequestRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, RepositoryAuditService? auditService = null)
    {
        _db = db; _sql = sql; _auditService = auditService ?? new RepositoryAuditService(auditWriter); _tracker = tracker;
    }

    public async Task<ApprovalRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var e = await conn.QuerySingleOrDefaultAsync<ApprovalRequest>(_sql.Get("Approval.Select.Request.ById"), new { Id = id });
        if (e != null) _tracker?.Track(e, "ApprovalRequests");
        return e;
    }

    public async Task<List<ApprovalRequest>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var list = (await conn.QueryAsync<ApprovalRequest>(_sql.Get("Approval.Select.Request.All"))).ToList();
        foreach (var e in list) _tracker?.Track(e, "ApprovalRequests");
        return list;
    }

    public async Task<ApprovalRequest> AddAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        if (entity.CreatedAt == default) entity.SetCreated(Guid.NewGuid(), ChinaTime.Now, null, null);
        _auditService.PopulateCreatedFields(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Approval.Insert.Request.Default"), entity);
        await _auditService.WriteCreateLogAsync("ApprovalRequests", entity, ct);
        return entity;
    }

    public async Task UpdateAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        _auditService.PopulateUpdatedFields(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Approval.Update.Request.Default"), entity);
        await _auditService.WriteUpdateAsync("ApprovalRequests", entity, ct);
    }

    public async Task DeleteAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Approval.Delete.Request.ById"), new { entity.Id });
        var oldReq = await GetByIdAsync(entity.Id, ct);
        if (oldReq != null) await _auditService.WriteDeleteAsync("ApprovalRequests", oldReq, ct);
    }

    public Task<PagedResult<ApprovalRequest>> GetPagedAsync(int page, int pageSize,
        Expression<Func<ApprovalRequest, bool>>? predicate = null,
        Func<IQueryable<ApprovalRequest>, IOrderedQueryable<ApprovalRequest>>? orderBy = null,
        CancellationToken ct = default) => throw new NotSupportedException();

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>(_sql.Get("Approval.Select.Request.Exists"), new { Id = id }) > 0;
    }

    public async Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var roleIds = (await conn.QueryAsync<Guid>(_sql.Get("Identity.Select.RoleIds.ByUserId"), new { UserId = userId })).ToList();
        if (roleIds.Count == 0) return new();

        var configs = await conn.QueryAsync(_sql.Get("Approval.Select.LevelConfigs.ByApprovalType"), new { Ids = roleIds });

        var results = new List<ApprovalRequest>();
        foreach (var c in configs)
        {
            var items = (await conn.QueryAsync<ApprovalRequest>(
                _sql.Get("Approval.Select.Request.PendingByTypeLevel"),
                new { TypeId = (Guid)c.ApprovalTypeId, Level = (int)c.LevelNo })).ToList();
            results.AddRange(items);
        }
        return results.Distinct().OrderByDescending(a => a.CreatedAt).ToList();
    }

    public async Task<List<ApprovalRequest>> GetByTargetAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<ApprovalRequest>(
            _sql.Get("Approval.Select.Request.ByTarget"),
            new { Id = targetEntityId, Type = targetEntityType })).ToList();
    }

    public async Task<List<ApprovalRequest>> GetByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<ApprovalRequest>(
            _sql.Get("Approval.Select.Request.ByApprover"), new { UserId = userId })).ToList();
    }

    public async Task<ApprovalRequest?> GetByIdWithRecordsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Approval.Select.Request.ByIdWithRecords"), new { Id = id });
        var entity = await multi.ReadSingleOrDefaultAsync<ApprovalRequest>();
        if (entity != null)
        {
            var records = (await multi.ReadAsync<ApprovalRecord>()).ToList();
            var field = typeof(ApprovalRequest).GetField("_records",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(entity, records);
        }
        return entity;
    }

    public async Task<PagedResult<ApprovalRequest>> GetHistoryAsync(Guid userId, string? keyword, string? status,
        int page, int pageSize, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();

        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword)) conditions.Add("AND a.Title LIKE @Keyword");
        if (!string.IsNullOrWhiteSpace(status)) conditions.Add("AND a.Status = @Status");
        var condSql = conditions.Count > 0 ? " " + string.Join(" ", conditions) : "";

        var countSql = string.Format(_sql.Get("Approval.Select.History.CountByUserId"), condSql);
        var total = await conn.QuerySingleAsync<int>(countSql,
            new { UserId = userId, Keyword = $"%{keyword}%", Status = status });

        var dataSql = string.Format(_sql.Get("Approval.Select.History.ByUserId"), condSql);
        var items = await conn.QueryAsync<ApprovalRequest>(dataSql,
            new { UserId = userId, Keyword = $"%{keyword}%", Status = status, Offset = (page - 1) * pageSize, PageSize = pageSize });

        return new PagedResult<ApprovalRequest> { Items = items.ToList(), Total = total, Page = page, PageSize = pageSize };
    }
}
