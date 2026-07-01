using Dapper;
using System.Linq.Expressions;
using RBS.Core.Common;
using RBS.Core.Entities.Organization;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

// =====================================================================
// Dapper 仓储 — IUserRepository
// =====================================================================

public class DapperUserRepository : IUserRepository
{
    protected readonly IDbConnectionFactory _db;
    public DapperUserRepository(IDbConnectionFactory db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id=@Id", new { Id = id });
    }
    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<User>("SELECT * FROM Users")).ToList();
    }
    public async Task<User> AddAsync(User entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("INSERT INTO Users (Id,Username,PasswordHash,DisplayName,Phone,Email,IsActive,HomeCompanyId,IsSuperAdmin,CreatedBy,CreatedAt) VALUES(@Id,@Username,@PasswordHash,@DisplayName,@Phone,@Email,@IsActive,@HomeCompanyId,@IsSuperAdmin,@CreatedBy,@CreatedAt)", entity);
        return entity;
    }
    public async Task UpdateAsync(User entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("UPDATE Users SET Username=@Username,DisplayName=@DisplayName,Phone=@Phone,Email=@Email,IsActive=@IsActive,HomeCompanyId=@HomeCompanyId,IsSuperAdmin=@IsSuperAdmin,UpdatedBy=@UpdatedBy,UpdatedAt=@UpdatedAt WHERE Id=@Id", entity);
    }
    public async Task DeleteAsync(User entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("DELETE FROM Users WHERE Id=@Id", new { entity.Id });
    }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM Users WHERE Id=@Id", new { Id = id }) > 0;
    }
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Username=@Username", new { Username = username });
    }
    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id=@Id", new { Id = id });
    }
    public async Task<List<User>> GetAllWithRolesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<User>("SELECT * FROM Users")).ToList();
    }
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<string>(@"SELECT DISTINCT m.PermissionCode FROM RoleMenus rm
            INNER JOIN Menus m ON m.Id = rm.MenuId
            INNER JOIN UserRoles ur ON ur.RoleId = rm.RoleId
            WHERE ur.UserId = @UserId AND m.PermissionCode IS NOT NULL", new { UserId = userId })).ToList();
    }
    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        if (excludeId.HasValue)
            return await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM Users WHERE Username=@Username AND Id!=@Id", new { Username = username, Id = excludeId }) == 0;
        return await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM Users WHERE Username=@Username", new { Username = username }) == 0;
    }
    public async Task ReplaceRolesAsync(Guid userId, List<Guid> newRoleIds, Guid changedBy, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync("DELETE FROM UserRoles WHERE UserId=@UserId", new { UserId = userId }, tx);
        foreach (var roleId in newRoleIds)
            await conn.ExecuteAsync("INSERT INTO UserRoles (Id,UserId,RoleId,CreatedBy,CreatedAt) VALUES(@Id,@UserId,@RoleId,@CreatedBy,@CreatedAt)",
                new { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, CreatedBy = changedBy, CreatedAt = ChinaTime.Now }, tx);
        tx.Commit();
    }
    public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, System.Linq.Expressions.Expression<Func<User, bool>>? predicate = null, Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

// =====================================================================
// Dapper 仓储 — IRoleRepository
// =====================================================================

public class DapperRoleRepository : IRoleRepository
{
    protected readonly IDbConnectionFactory _db;
    public DapperRoleRepository(IDbConnectionFactory db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Role>("SELECT * FROM Roles WHERE Id=@Id", new { Id = id }); }
    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Role>("SELECT * FROM Roles")).ToList(); }
    public async Task<Role> AddAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("INSERT INTO Roles (Id,Name,Code,Description,IsActive,CreatedBy,CreatedAt) VALUES(@Id,@Name,@Code,@Description,@IsActive,@CreatedBy,@CreatedAt)", entity); return entity; }
    public async Task UpdateAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("UPDATE Roles SET Name=@Name,Code=@Code,Description=@Description,IsActive=@IsActive,UpdatedBy=@UpdatedBy,UpdatedAt=@UpdatedAt WHERE Id=@Id", entity); }
    public async Task DeleteAsync(Role entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("DELETE FROM Roles WHERE Id=@Id", new { entity.Id }); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM Roles WHERE Id=@Id", new { Id = id }) > 0; }
    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Role>("SELECT * FROM Roles WHERE Code=@Code", new { Code = code }); }
    public async Task<List<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Role>("SELECT r.* FROM Roles r INNER JOIN UserRoles ur ON ur.RoleId=r.Id WHERE ur.UserId=@UserId", new { UserId = userId })).ToList(); }
    public async Task<Role?> GetByIdWithRoleMenusAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Role>("SELECT * FROM Roles WHERE Id=@Id", new { Id = id }); }
    public Task<PagedResult<Role>> GetPagedAsync(int page, int pageSize, System.Linq.Expressions.Expression<Func<Role, bool>>? predicate = null, Func<IQueryable<Role>, IOrderedQueryable<Role>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

// =====================================================================
// Dapper 仓储 — IMenuRepository
// =====================================================================

public class DapperMenuRepository : IMenuRepository
{
    protected readonly IDbConnectionFactory _db;
    public DapperMenuRepository(IDbConnectionFactory db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

    public async Task<Menu?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Menu>("SELECT * FROM Menus WHERE Id=@Id", new { Id = id }); }
    public async Task<List<Menu>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Menu>("SELECT * FROM Menus ORDER BY SortOrder")).ToList(); }
    public async Task<Menu> AddAsync(Menu entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("INSERT INTO Menus (Id,Name,PermissionCode,Path,Icon,ParentId,SortOrder,IsActive,Scope,CreatedBy,CreatedAt) VALUES(@Id,@Name,@PermissionCode,@Path,@Icon,@ParentId,@SortOrder,@IsActive,@Scope,@CreatedBy,@CreatedAt)", entity); return entity; }
    public async Task UpdateAsync(Menu entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("UPDATE Menus SET Name=@Name,Path=@Path,Icon=@Icon,SortOrder=@SortOrder,IsActive=@IsActive,UpdatedBy=@UpdatedBy,UpdatedAt=@UpdatedAt WHERE Id=@Id", entity); }
    public async Task DeleteAsync(Menu entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("DELETE FROM Menus WHERE Id=@Id", new { entity.Id }); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM Menus WHERE Id=@Id", new { Id = id }) > 0; }
    public async Task<List<Menu>> GetByRoleIdsAsync(List<Guid> roleIds, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Menu>("SELECT DISTINCT m.* FROM Menus m INNER JOIN RoleMenu rm ON rm.MenuId=m.Id WHERE rm.RoleId IN @Ids ORDER BY m.SortOrder", new { Ids = roleIds })).ToList(); }
    public async Task<List<Menu>> GetTreeAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Menu>("SELECT * FROM Menus ORDER BY SortOrder")).ToList(); }
    public Task<PagedResult<Menu>> GetPagedAsync(int page, int pageSize, System.Linq.Expressions.Expression<Func<Menu, bool>>? predicate = null, Func<IQueryable<Menu>, IOrderedQueryable<Menu>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

// =====================================================================
// Dapper 仓储 — ICompanyRepository
// =====================================================================

public class DapperCompanyRepository : ICompanyRepository
{
    protected readonly IDbConnectionFactory _db;
    public DapperCompanyRepository(IDbConnectionFactory db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Company>("SELECT * FROM Companies WHERE Id=@Id", new { Id = id }); }
    public async Task<List<Company>> GetAllAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Company>("SELECT * FROM Companies")).ToList(); }
    public async Task<Company> AddAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("INSERT INTO Companies (Id,Name,Code,ContactPerson,Phone,Address,IdType,IdNumber,BankName,BankAccount,BankAccountName,SettlementCycle,SettlementDay,CommissionRate,Remark,IsActive,CreatedBy,CreatedAt) VALUES(@Id,@Name,@Code,@ContactPerson,@Phone,@Address,@IdType,@IdNumber,@BankName,@BankAccount,@BankAccountName,@SettlementCycle,@SettlementDay,@CommissionRate,@Remark,@IsActive,@CreatedBy,@CreatedAt)", entity); return entity; }
    public async Task UpdateAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("UPDATE Companies SET Name=@Name,Code=@Code,ContactPerson=@ContactPerson,Phone=@Phone,Address=@Address,IsActive=@IsActive,UpdatedBy=@UpdatedBy,UpdatedAt=@UpdatedAt WHERE Id=@Id", entity); }
    public async Task DeleteAsync(Company entity, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync("DELETE FROM Companies WHERE Id=@Id", new { entity.Id }); }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM Companies WHERE Id=@Id", new { Id = id }) > 0; }
    public async Task<Company?> GetByNameAsync(string name, CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<Company>("SELECT * FROM Companies WHERE Name=@Name", new { Name = name }); }
    public async Task<List<Company>> GetActiveAsync(CancellationToken ct = default) { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<Company>("SELECT * FROM Companies WHERE IsActive=1")).ToList(); }
    public Task<PagedResult<Company>> GetPagedAsync(int page, int pageSize, System.Linq.Expressions.Expression<Func<Company, bool>>? predicate = null, Func<IQueryable<Company>, IOrderedQueryable<Company>>? orderBy = null, CancellationToken ct = default)
        => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");
}

public class DapperApprovalRequestRepository : IApprovalRequestRepository
{
    private readonly IDbConnectionFactory _db;
    public DapperApprovalRequestRepository(IDbConnectionFactory db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

    public async Task<ApprovalRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<ApprovalRequest>("SELECT * FROM ApprovalRequests WHERE Id=@Id", new { Id = id });
    }

    public async Task<List<ApprovalRequest>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<ApprovalRequest>("SELECT * FROM ApprovalRequests ORDER BY CreatedAt DESC")).ToList();
    }

    public async Task<ApprovalRequest> AddAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(@"
            INSERT INTO ApprovalRequests (Id, ApprovalTypeId, Title, Description, TargetEntityId, TargetEntityType,
                CurrentLevel, MaxLevel, Status, CompanyId, CreatedBy, CreatedAt)
            VALUES (@Id, @ApprovalTypeId, @Title, @Description, @TargetEntityId, @TargetEntityType,
                @CurrentLevel, @MaxLevel, @Status, @CompanyId, @CreatedBy, @CreatedAt)", entity);
        return entity;
    }

    public async Task UpdateAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(@"UPDATE ApprovalRequests SET Status=@Status,CurrentLevel=@CurrentLevel,
            UpdatedBy=@UpdatedBy,UpdatedAt=@UpdatedAt WHERE Id=@Id", entity);
    }

    public async Task DeleteAsync(ApprovalRequest entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("DELETE FROM ApprovalRequests WHERE Id=@Id", new { entity.Id });
    }

    public Task<PagedResult<ApprovalRequest>> GetPagedAsync(int page, int pageSize,
        System.Linq.Expressions.Expression<Func<ApprovalRequest, bool>>? predicate = null,
        Func<IQueryable<ApprovalRequest>, IOrderedQueryable<ApprovalRequest>>? orderBy = null,
        CancellationToken ct = default) => throw new NotSupportedException();

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM ApprovalRequests WHERE Id=@Id", new { Id = id }) > 0;
    }

    public async Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var roleIds = (await conn.QueryAsync<Guid>("SELECT RoleId FROM UserRoles WHERE UserId=@UserId", new { UserId = userId })).ToList();
        if (roleIds.Count == 0) return new();

        var configs = await conn.QueryAsync("SELECT DISTINCT ApprovalTypeId, [Level] FROM ApprovalLevelConfigs WHERE RoleId IN @Ids",
            new { Ids = roleIds });

        var results = new List<ApprovalRequest>();
        foreach (var c in configs)
        {
            var items = (await conn.QueryAsync<ApprovalRequest>(@"
                SELECT * FROM ApprovalRequests WHERE Status='Pending' AND ApprovalTypeId=@TypeId AND CurrentLevel=@Level
                ORDER BY CreatedAt DESC", new { TypeId = (Guid)c.ApprovalTypeId, Level = (int)c.Level })).ToList();
            results.AddRange(items);
        }
        return results.Distinct().OrderByDescending(a => a.CreatedAt).ToList();
    }

    public async Task<List<ApprovalRequest>> GetByTargetAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<ApprovalRequest>(
            "SELECT * FROM ApprovalRequests WHERE TargetEntityId=@Id AND TargetEntityType=@Type ORDER BY CreatedAt DESC",
            new { Id = targetEntityId, Type = targetEntityType })).ToList();
    }

    public async Task<List<ApprovalRequest>> GetByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<ApprovalRequest>(@"
            SELECT a.* FROM ApprovalRequests a
            WHERE EXISTS (SELECT 1 FROM ApprovalRecords r WHERE r.ApprovalRequestId=a.Id AND r.ApproverId=@UserId AND r.Action='Submitted')
            ORDER BY a.CreatedAt DESC", new { UserId = userId })).ToList();
    }

    public async Task<ApprovalRequest?> GetByIdWithRecordsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            "SELECT * FROM ApprovalRequests WHERE Id=@Id; SELECT * FROM ApprovalRecords WHERE ApprovalRequestId=@Id ORDER BY CreatedAt",
            new { Id = id });
        var entity = await multi.ReadSingleOrDefaultAsync<ApprovalRequest>();
        if (entity != null)
        {
            var records = (await multi.ReadAsync<ApprovalRecord>()).ToList();
            var field = typeof(ApprovalRequest).GetField("_records", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(entity, records);
        }
        return entity;
    }

    public async Task<PagedResult<ApprovalRequest>> GetHistoryAsync(Guid userId, string? keyword, string? status,
        int page, int pageSize, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var sql = @"SELECT * FROM ApprovalRequests a
            WHERE EXISTS (SELECT 1 FROM ApprovalRecords r WHERE r.ApprovalRequestId=a.Id AND r.ApproverId=@UserId
                AND (r.Action='Approved' OR r.Action='Rejected'))";

        if (!string.IsNullOrWhiteSpace(keyword)) sql += " AND a.Title LIKE @Keyword";
        if (!string.IsNullOrWhiteSpace(status)) sql += " AND a.Status = @Status";

        var total = await conn.QuerySingleAsync<int>(sql.Replace("SELECT *", "SELECT COUNT(*)"),
            new { UserId = userId, Keyword = $"%{keyword}%", Status = status });

        sql += " ORDER BY a.CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = await conn.QueryAsync<ApprovalRequest>(sql,
            new { UserId = userId, Keyword = $"%{keyword}%", Status = status, Offset = (page - 1) * pageSize, PageSize = pageSize });

        return new PagedResult<ApprovalRequest> { Items = items.ToList(), Total = total, Page = page, PageSize = pageSize };
    }
}

// ===== Dapper 仓储 — IFeeCodeRepository =====
public class DapperFeeCodeRepository : DapperRepository<FeeCode>, IFeeCodeRepository
{
    public DapperFeeCodeRepository(IDbConnectionFactory db) : base(db, "FeeCodes") { }
    public async Task<FeeCode?> GetByCodeAsync(string code, Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<FeeCode>("SELECT * FROM FeeCodes WHERE Code=@Code", new { Code = code }); }
    public async Task<List<FeeCode>> GetByCategoryAsync(string category, Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<FeeCode>("SELECT * FROM FeeCodes WHERE Category=@Category", new { Category = category })).ToList(); }
}

// ===== Dapper 仓储 — IPaymentChannelRepository =====
public class DapperPaymentChannelRepository : DapperRepository<PaymentChannel>, IPaymentChannelRepository
{
    public DapperPaymentChannelRepository(IDbConnectionFactory db) : base(db, "PaymentChannels") { }
    public async Task<List<PaymentChannel>> GetActiveByCompanyAsync(Guid companyId, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<PaymentChannel>("SELECT * FROM PaymentChannels WHERE IsActive=1")).ToList(); }
}

// ===== Dapper 仓储 — IHolidayCalendarRepository =====
public class DapperHolidayCalendarRepository : DapperRepository<HolidayCalendar>, IHolidayCalendarRepository
{
    public DapperHolidayCalendarRepository(IDbConnectionFactory db) : base(db, "HolidayCalendars") { }
    public async Task<List<HolidayCalendar>> GetByYearAsync(Guid companyId, int year, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return (await conn.QueryAsync<HolidayCalendar>("SELECT * FROM HolidayCalendars WHERE YEAR(HolidayDate)=@Year ORDER BY HolidayDate", new { Year = year })).ToList(); }
    public async Task<HolidayCalendar?> GetByDateAsync(Guid companyId, DateOnly date, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); return await conn.QuerySingleOrDefaultAsync<HolidayCalendar>("SELECT * FROM HolidayCalendars WHERE HolidayDate=@Date", new { Date = date }); }
}
