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

public class DapperApprovalRequestRepository : IApprovalRequestRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IChangeTracker? _tracker;
    public DapperApprovalRequestRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null) { _db = db; _sql = sql; _auditWriter = auditWriter; _tracker = tracker; }

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
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Approval.Insert.Request.Default"), entity);
        await _auditWriter.LogChangesAsync("ApprovalRequests", entity.Id.ToString(), "Create", new() { ["Title"] = entity.Title, ["Status"] = entity.Status ?? "Pending" }, entity.CreatedBy, ct);
        return entity;
    }

    public async Task UpdateAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Approval.Update.Request.Default"), entity);
        await _auditWriter.LogChangesAsync("ApprovalRequests", entity.Id.ToString(), "Update", new() { ["Status"] = entity.Status }, entity.UpdatedBy ?? entity.CreatedBy, ct);
    }

    public async Task DeleteAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Approval.Delete.Request.ById"), new { entity.Id });
        await _auditWriter.LogChangesAsync("ApprovalRequests", entity.Id.ToString(), "Delete", new() { ["Id"] = entity.Id.ToString() }, Guid.Empty, ct);
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

    /// <summary>
    /// 查询指定审批人待审批的请求列表
    /// </summary>
    /// <remarks>
    /// 查询策略：
    /// 1. 先查出用户的所有角色 ID
    /// 2. 根据角色查找对应的审批层级配置（ApprovalLevelConfigs）
    /// 3. 按审批类型+级别逐类查询待审批请求
    /// 4. 去重后按创建时间降序排列
    /// </remarks>
    /// <param name="userId">审批人用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>待审批请求列表</returns>
    public async Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var roleIds = (await conn.QueryAsync<Guid>(_sql.Get("Identity.Select.RoleIds.ByUserId"), new { UserId = userId })).ToList();
        if (roleIds.Count == 0) return new();

        var configs = await conn.QueryAsync(_sql.Get("Approval.Select.LevelConfigs.ByApprovalType"),
            new { Ids = roleIds });

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

    /// <summary>
    /// 根据目标实体 ID 和类型查询关联的审批请求
    /// </summary>
    /// <param name="targetEntityId">目标实体 ID（如合同 ID）</param>
    /// <param name="targetEntityType">目标实体类型（如 "Contract"）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>关联的审批请求列表</returns>
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

    /// <summary>
    /// 根据 ID 查询审批请求及其关联的审批记录
    /// </summary>
    /// <remarks>
    /// 使用 QueryMultipleAsync 一次性加载主表和子表数据，
    /// 通过反射将 ApprovalRecord 列表写入审批请求的私有 _records 字段。
    /// </remarks>
    /// <param name="id">审批请求 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>含审批记录的审批请求，未找到时返回 null</returns>
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

    /// <summary>
    /// 分页查询审批历史记录（含关键词和状态筛选）
    /// </summary>
    /// <remarks>
    /// SQL 策略：使用 string.Format 动态拼接 WHERE 子句，
    /// 先 COUNT 查总数再 OFFSET-FETCH 分页查询。
    /// 关键词和状态均为可选参数。
    /// </remarks>
    /// <param name="userId">用户 ID</param>
    /// <param name="keyword">关键词（模糊匹配标题）</param>
    /// <param name="status">审批状态筛选</param>
    /// <param name="page">页码（从 1 开始）</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>分页结果</returns>
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
