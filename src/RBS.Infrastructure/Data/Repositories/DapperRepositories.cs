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

// =====================================================================
// Dapper 仓储 — IUserRepository
// =====================================================================

public class DapperUserRepository : IUserRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IChangeTracker? _tracker;

    public DapperUserRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null)
    {
        _db = db;
        _sql = sql;
        _auditWriter = auditWriter;
        _tracker = tracker;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var entity = await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ById"), new { Id = id });
        if (entity != null) _tracker?.Track(entity, "Users");
        return entity;
    }
    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var list = (await conn.QueryAsync<User>(_sql.Get("Identity.Select.User.All"))).ToList();
        foreach (var e in list) _tracker?.Track(e, "Users");
        return list;
    }
    public async Task<User> AddAsync(User entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Insert.User.Default"), entity);
        var changes = new Dictionary<string, object?> { ["Username"] = entity.Username, ["DisplayName"] = entity.DisplayName };
        await _auditWriter.LogChangesAsync("Users", entity.Id.ToString(), "Create", changes, entity.CreatedBy, ct);
        return entity;
    }
    public async Task UpdateAsync(User entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Update.User.Default"), entity);
        var changes = new Dictionary<string, object?> { ["DisplayName"] = entity.DisplayName, ["Phone"] = entity.Phone, ["Email"] = entity.Email };
        await _auditWriter.LogChangesAsync("Users", entity.Id.ToString(), "Update", changes, entity.UpdatedBy ?? Guid.Empty, ct);
    }
    public async Task DeleteAsync(User entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Delete.User.ById"), new { entity.Id });
        await _auditWriter.LogChangesAsync("Users", entity.Id.ToString(), "Delete", new() { ["Id"] = entity.Id.ToString() }, Guid.Empty, ct);
    }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>(_sql.Get("Identity.Select.User.Exists"), new { Id = id }) > 0;
    }
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ByUsername"), new { Username = username });
    }
    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ById"), new { Id = id });
    }
    public async Task<List<User>> GetAllWithRolesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Identity.Select.User.AllWithRoles"));
        var users = (await multi.ReadAsync<User>()).ToList();
        var roleRows = (await multi.ReadAsync<dynamic>()).ToList();
        var roleField = typeof(User).GetField("_roles",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (roleField != null)
        {
            foreach (var u in users)
            {
                var urs = roleRows.Where(r => r.UserId == u.Id).Select(r => new UserRole(u.Id, (Guid)r.RoleId)).ToList();
                roleField.SetValue(u, urs);
            }
        }
        return users;
    }
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<string>(_sql.Get("Identity.Select.Permission.ByUserId"), new { UserId = userId })).ToList();
    }
    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        if (excludeId.HasValue)
            return await conn.QuerySingleAsync<int>(_sql.Get("Identity.Select.User.UsernameUniqueExclude"), new { Username = username, Id = excludeId }) == 0;
        return await conn.QuerySingleAsync<int>(_sql.Get("Identity.Select.User.UsernameUnique"), new { Username = username }) == 0;
    }
    public async Task ReplaceRolesAsync(Guid userId, List<Guid> newRoleIds, Guid changedBy, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(_sql.Get("Identity.Delete.UserRoles.ByUserId"), new { UserId = userId }, tx);
        foreach (var roleId in newRoleIds)
            await conn.ExecuteAsync(_sql.Get("Identity.Insert.UserRole.Default"),
                new { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, CreatedBy = changedBy, CreatedAt = ChinaTime.Now }, tx);
        tx.Commit();
        await _auditWriter.LogChangesAsync("UserRoles", userId.ToString(), "Update", new() { ["RoleIds"] = string.Join(",", newRoleIds) }, changedBy, ct);
    }
    public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, Expression<Func<User, bool>>? predicate = null, Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

// =====================================================================
// Dapper 仓储 — IRoleRepository
// =====================================================================

public class DapperRoleRepository : IRoleRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IChangeTracker? _tracker;
    public DapperRoleRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null) { _db = db; _sql = sql; _auditWriter = auditWriter; _tracker = tracker; }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var e = await conn.QuerySingleOrDefaultAsync<Role>(_sql.Get("Authorization.Select.Role.ById"), new { Id = id }); if (e != null) _tracker?.Track(e, "Roles"); return e; }
    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var list = (await conn.QueryAsync<Role>(_sql.Get("Authorization.Select.Role.All"))).ToList(); foreach (var e in list) _tracker?.Track(e, "Roles"); return list; }
    public async Task<Role> AddAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Insert.Role.Default"), entity); await _auditWriter.LogChangesAsync("Roles", entity.Id.ToString(), "Create", new() { ["Name"] = entity.Name, ["Code"] = entity.Code }, entity.CreatedBy, ct); return entity; }
    public async Task UpdateAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Update.Role.Default"), entity); await _auditWriter.LogChangesAsync("Roles", entity.Id.ToString(), "Update", new() { ["Name"] = entity.Name }, entity.UpdatedBy ?? Guid.Empty, ct); }
    public async Task DeleteAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Delete.Role.ById"), new { entity.Id }); await _auditWriter.LogChangesAsync("Roles", entity.Id.ToString(), "Delete", new() { ["Id"] = entity.Id.ToString() }, Guid.Empty, ct); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Authorization.Select.Role.Exists"), new { Id = id }) > 0; }
    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Role>(_sql.Get("Authorization.Select.Role.ByCode"), new { Code = code }); }
    public async Task<List<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Role>(_sql.Get("Authorization.Select.Role.ByUserId"), new { UserId = userId })).ToList(); }
    public async Task<Role?> GetByIdWithRoleMenusAsync(Guid id, CancellationToken ct = default) {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Authorization.Select.Role.WithMenus"), new { Id = id });
        var role = await multi.ReadSingleOrDefaultAsync<Role>();
        if (role != null)
        {
            var menuIds = (await multi.ReadAsync<Guid>()).ToList();
            var field = typeof(Role).GetField("_roleMenus",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var menus = menuIds.Select(mid => new RoleMenu(role.Id, mid)).ToList();
                field.SetValue(role, menus);
            }
        }
        return role;
    }
    public Task<PagedResult<Role>> GetPagedAsync(int page, int pageSize, Expression<Func<Role, bool>>? predicate = null, Func<IQueryable<Role>, IOrderedQueryable<Role>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

// =====================================================================
// Dapper 仓储 — IMenuRepository
// =====================================================================

public class DapperMenuRepository : IMenuRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IChangeTracker? _tracker;
    public DapperMenuRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null) { _db = db; _sql = sql; _auditWriter = auditWriter; _tracker = tracker; }

    public async Task<Menu?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var e = await conn.QuerySingleOrDefaultAsync<Menu>(_sql.Get("Authorization.Select.Menu.ById"), new { Id = id }); if (e != null) _tracker?.Track(e, "Menus"); return e; }
    public async Task<List<Menu>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var list = (await conn.QueryAsync<Menu>(_sql.Get("Authorization.Select.Menu.All"))).ToList(); foreach (var e in list) _tracker?.Track(e, "Menus"); return list; }
    public async Task<Menu> AddAsync(Menu entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Insert.Menu.Default"), entity); await _auditWriter.LogChangesAsync("Menus", entity.Id.ToString(), "Create", new() { ["Name"] = entity.Name, ["PermissionCode"] = entity.PermissionCode }, entity.CreatedBy, ct); return entity; }
    public async Task UpdateAsync(Menu entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Update.Menu.Default"), entity); await _auditWriter.LogChangesAsync("Menus", entity.Id.ToString(), "Update", new() { ["Name"] = entity.Name }, entity.UpdatedBy ?? Guid.Empty, ct); }
    public async Task DeleteAsync(Menu entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Authorization.Delete.Menu.ById"), new { entity.Id }); await _auditWriter.LogChangesAsync("Menus", entity.Id.ToString(), "Delete", new() { ["Id"] = entity.Id.ToString() }, Guid.Empty, ct); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Authorization.Select.Menu.Exists"), new { Id = id }) > 0; }
    public async Task<List<Menu>> GetByRoleIdsAsync(List<Guid> roleIds, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Menu>(_sql.Get("Authorization.Select.Menu.ByRoleIds"), new { Ids = roleIds })).ToList(); }
    public async Task<List<Menu>> GetTreeAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Menu>(_sql.Get("Authorization.Select.Menu.All"))).ToList(); }
    public Task<PagedResult<Menu>> GetPagedAsync(int page, int pageSize, Expression<Func<Menu, bool>>? predicate = null, Func<IQueryable<Menu>, IOrderedQueryable<Menu>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

// =====================================================================
// Dapper 仓储 — ICompanyRepository
// =====================================================================

public class DapperCompanyRepository : ICompanyRepository
{
    protected readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IChangeTracker? _tracker;
    public DapperCompanyRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null) { _db = db; _sql = sql; _auditWriter = auditWriter; _tracker = tracker; }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var e = await conn.QuerySingleOrDefaultAsync<Company>(_sql.Get("Organization.Select.Company.ById"), new { Id = id }); if (e != null) _tracker?.Track(e, "Companies"); return e; }
    public async Task<List<Company>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); var list = (await conn.QueryAsync<Company>(_sql.Get("Organization.Select.Company.All"))).ToList(); foreach (var e in list) _tracker?.Track(e, "Companies"); return list; }
    public async Task<Company> AddAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Organization.Insert.Company.Default"), entity); await _auditWriter.LogChangesAsync("Companies", entity.Id.ToString(), "Create", new() { ["Name"] = entity.Name, ["Code"] = entity.Code }, entity.CreatedBy, ct); return entity; }
    public async Task UpdateAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Organization.Update.Company.Default"), entity); await _auditWriter.LogChangesAsync("Companies", entity.Id.ToString(), "Update", new() { ["Name"] = entity.Name }, entity.UpdatedBy ?? Guid.Empty, ct); }
    public async Task DeleteAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Organization.Delete.Company.ById"), new { entity.Id }); await _auditWriter.LogChangesAsync("Companies", entity.Id.ToString(), "Delete", new() { ["Id"] = entity.Id.ToString() }, Guid.Empty, ct); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Organization.Select.Company.Exists"), new { Id = id }) > 0; }
    public async Task<Company?> GetByNameAsync(string name, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Company>(_sql.Get("Organization.Select.Company.ByName"), new { Name = name }); }
    public async Task<List<Company>> GetActiveAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Company>(_sql.Get("Organization.Select.Company.Active"))).ToList(); }
    public Task<PagedResult<Company>> GetPagedAsync(int page, int pageSize, Expression<Func<Company, bool>>? predicate = null, Func<IQueryable<Company>, IOrderedQueryable<Company>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

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

// ===== Dapper 仓储 — IFeeCodeRepository =====
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

// ===== Dapper 仓储 — IPaymentChannelRepository =====
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

// ===== Dapper 仓储 — IHolidayCalendarRepository =====
public class DapperHolidayCalendarRepository : DapperRepository<HolidayCalendar>, IHolidayCalendarRepository
{
    private readonly ISqlLoader _sql;
    public DapperHolidayCalendarRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, "HolidayCalendars", tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<List<HolidayCalendar>> GetByYearAsync(Guid companyId, int year, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<HolidayCalendar>(_sql.Get("Calendar.Select.Holiday.ByYear"), new { Year = year })).ToList(); }
    public async Task<HolidayCalendar?> GetByDateAsync(Guid companyId, DateOnly date, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<HolidayCalendar>(_sql.Get("Calendar.Select.Holiday.ByDate"), new { Date = date }); }
}

public class DapperTenantRepository : DapperRepository<Tenant>, ITenantRepository
{
    private readonly ISqlLoader _sql;
    public DapperTenantRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, tracker: tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<Tenant?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Tenant>(_sql.Get("Rental.Select.Tenant.ByPhone"), new { Phone = phone }); }
    public async Task<List<Tenant>> SearchAsync(string keyword, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Tenant>(_sql.Get("Rental.Select.Tenant.Search"), new { K = $"%{keyword}%" })).ToList(); }
}

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

public class DapperReceivablePlanRepository : DapperRepository<ReceivablePlan>, IReceivablePlanRepository
{
    private readonly ISqlLoader _sql;
    public DapperReceivablePlanRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, tracker: tracker, tenant: tenant)
    {
        _sql = sql;
    }
    public async Task<List<ReceivablePlan>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<ReceivablePlan>(_sql.Get("Receivable.Select.Plan.ByContractId"), new { Id = contractId })).ToList(); }
    public async Task<ReceivablePlan?> GetByContractPeriodFeeAsync(Guid contractId, string period, Guid feeCodeId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<ReceivablePlan>(_sql.Get("Receivable.Select.Plan.ByContractPeriodFee"), new { Id = contractId, P = period, F = feeCodeId }); }
    public async Task<List<ReceivablePlan>> GetOverdueAsync(Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<ReceivablePlan>(_sql.Get("Receivable.Select.Plan.Overdue"))).ToList(); }
}

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
}

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
    public async Task<List<Contract>> GetContractsExpiringAsync(DateOnly date, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Contract>(_sql.Get("Lease.Select.Contract.Expiring"), new { Date = date })).ToList(); }
    public async Task<bool> HasActiveForHousingUnitAsync(Guid housingUnitId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>(_sql.Get("Lease.Select.Contract.HasActiveForHousingUnit"), new { Id = housingUnitId }) > 0; }
}

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

// ===== Dapper 仓储 — IApprovalBizDataRepository =====
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

// ===== Dapper 仓储 — IApprovalFeeItemRepository =====
public class DapperApprovalFeeItemRepository : DapperRepository<ApprovalFeeItem>, IApprovalFeeItemRepository
{
    private readonly ISqlLoader _sql;
    public DapperApprovalFeeItemRepository(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, IChangeTracker? tracker = null, ITenantService? tenant = null) : base(db, auditWriter, "ApprovalFeeItems", tracker, tenant: tenant)
    {
        _sql = sql;
    }

    public async Task<List<ApprovalFeeItem>> GetByApprovalRequestIdAsync(Guid approvalRequestId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<ApprovalFeeItem>(_sql.Get("Approval.Select.ApprovalFeeItems.ByApprovalRequestId"), new { Id = approvalRequestId })).ToList(); }
}
