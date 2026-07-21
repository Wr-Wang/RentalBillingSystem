using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Common;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using System.Data;
using BCryptNet = BCrypt.Net.BCrypt;

namespace RBS.Application.Services.Organization;

public class UserService : IUserService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public UserService(IDbConnectionFactory db, ISqlLoader sql, ICurrentUserService currentUserService, IUnitOfWork uow)
    {
        _db = db; _sql = sql; _currentUserService = currentUserService; _uow = uow;
    }

    public async Task<List<UserDto>> GetListAsync(Guid? companyId = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();

        // 非超管只能看自己公司的用户（数据隔离），忽略前端传入的 companyId
        Guid? effectiveCompanyId;
        if (!_currentUserService.IsSuperAdmin)
            effectiveCompanyId = _currentUserService.CompanyId;
        else
            effectiveCompanyId = companyId;

        var sqlKey = effectiveCompanyId.HasValue ? "Identity.Select.User.ByCompanyId" : "Identity.Select.User.All";
        var param = effectiveCompanyId.HasValue ? new { CompanyId = effectiveCompanyId.Value } : null;
        var users = (await conn.QueryAsync<User>(_sql.Get(sqlKey), param)).ToList();
        var dtos = new List<UserDto>();
        foreach (var user in users)
            dtos.Add(await MapToDtoAsync(user, conn, ct));
        return dtos;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var user = await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ById"), new { Id = id });
        return user == null ? null : await MapToDtoAsync(user, conn, ct);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var unique = await _uow.Users.IsUsernameUniqueAsync(request.Username, null, ct);
        if (!unique) throw new InvalidOperationException($"用户名 '{request.Username}' 已存在");

        var user = new User(request.Username, request.DisplayName, BCryptNet.HashPassword(request.Password));
        if (request.IsSuperAdmin) user.GrantSuperAdmin();
        if (request.CompanyId.HasValue) user.SetCompany(request.CompanyId.Value);
        // 创建时同时设置 Phone/Email（UpdateProfile 一并更新 DisplayName，传当前值不影响）
        user.UpdateProfile(user.DisplayName, request.Phone, request.Email);

        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(_sql.Get("Identity.Insert.User.Default"),
                new { user.Id, user.Username, user.PasswordHash, user.DisplayName,
                    user.Phone, user.Email, user.IsActive, user.CompanyId, user.IsSuperAdmin,
                    CreatedBy = _currentUserService.UserId, CreatedAt = ChinaTime.Now });

            if (request.RoleIds?.Any() == true)
            {
                foreach (var roleId in request.RoleIds)
                    await conn.ExecuteAsync(_sql.Get("Identity.Insert.UserRole.Default"),
                        new { Id = Guid.NewGuid(), UserId = user.Id, RoleId = roleId,
                            CreatedBy = _currentUserService.UserId, CreatedAt = ChinaTime.Now }, tx);
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }

        return (await GetByIdAsync(user.Id, ct))!;
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var user = await conn.QuerySingleOrDefaultAsync<User>(_sql.Get("Identity.Select.User.ById"), new { Id = id })
            ?? throw new KeyNotFoundException("用户不存在");

        if (request.DisplayName != null || request.Phone != null || request.Email != null)
            user.UpdateProfile(request.DisplayName ?? user.DisplayName, request.Phone, request.Email);
        if (!string.IsNullOrEmpty(request.Password)) user.ChangePassword(BCryptNet.HashPassword(request.Password));
        if (request.IsActive.HasValue) { if (request.IsActive.Value) user.Activate(); else user.Deactivate(); }
        if (request.CompanyId.HasValue && request.CompanyId.Value != user.CompanyId) user.SetCompany(request.CompanyId.Value);
        if (request.IsSuperAdmin.HasValue && request.IsSuperAdmin.Value != user.IsSuperAdmin) { if (request.IsSuperAdmin.Value) user.GrantSuperAdmin(); else user.RevokeSuperAdmin(); }

        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(_sql.Get("Identity.Update.User.Default"),
                new { user.DisplayName, user.PasswordHash, user.Phone, user.Email,
                    user.IsActive, user.CompanyId, user.IsSuperAdmin,
                    UpdatedBy = _currentUserService.UserId, UpdatedAt = ChinaTime.Now, Id = id }, tx);

            if (request.RoleIds != null)
            {
                await conn.ExecuteAsync(_sql.Get("Identity.Delete.UserRoles.ByUserId"), new { UserId = id }, tx);
                foreach (var roleId in request.RoleIds)
                    await conn.ExecuteAsync(_sql.Get("Identity.Insert.UserRole.Default"),
                        new { Id = Guid.NewGuid(), UserId = id, RoleId = roleId,
                            CreatedBy = _currentUserService.UserId, CreatedAt = ChinaTime.Now }, tx);
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public async Task SetDefaultCompanyAsync(Guid userId, Guid? companyId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Update.User.DefaultCompanyId"),
            new { Id = userId, CompanyId = companyId });
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Identity.Update.User.Deactivate"), new { Id = id });
    }

    private async Task<UserDto> MapToDtoAsync(User user, IDbConnection conn, CancellationToken ct)
    {
        var dto = new UserDto
        {
            Id = user.Id, Username = user.Username, DisplayName = user.DisplayName,
            Phone = user.Phone, Email = user.Email, IsActive = user.IsActive,
            CompanyId = user.CompanyId, IsSuperAdmin = user.IsSuperAdmin,
            CreatedAt = user.CreatedAt, RoleIds = new List<Guid>(), RoleNames = new List<string>()
        };

        var roles = (await conn.QueryAsync(
            _sql.Get("Identity.Select.UserRoles.ByUserId"),
            new { UserId = user.Id })).ToList();
        foreach (var row in roles)
        {
            dto.RoleIds.Add((Guid)row.RoleId);
            if (!string.IsNullOrEmpty(row.Name))
                dto.RoleNames.Add((string)row.Name);
        }

        if (user.CompanyId.HasValue)
        {
            dto.HomeCompanyName = await conn.QuerySingleOrDefaultAsync<string>(
                _sql.Get("Organization.Select.Company.NameById"), new { Id = user.CompanyId.Value });
        }

        return dto;
    }
}
