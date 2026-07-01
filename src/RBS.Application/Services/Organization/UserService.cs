using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.Interfaces.Services;
using RBS.Core.Common;
using System.Data;

namespace RBS.Application.Services.Organization;

public class UserService : IUserService
{
    private readonly IDbConnectionFactory _db;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public UserService(IDbConnectionFactory db, ICurrentUserService currentUserService, IUnitOfWork uow)
    {
        _db = db;
        _currentUserService = currentUserService;
        _uow = uow;
    }

    public async Task<List<UserDto>> GetListAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var users = (await conn.QueryAsync<User>("SELECT * FROM Users ORDER BY CreatedAt DESC")).ToList();
        var dtos = new List<UserDto>();
        foreach (var user in users)
            dtos.Add(await MapToDtoAsync(user, conn, ct));
        return dtos;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var user = await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id=@Id", new { Id = id });
        return user == null ? null : await MapToDtoAsync(user, conn, ct);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var unique = await _uow.Users.IsUsernameUniqueAsync(request.Username, null, ct);
        if (!unique) throw new InvalidOperationException($"用户名 '{request.Username}' 已存在");

        var user = new User(request.Username, request.DisplayName, request.Password);
        if (request.IsSuperAdmin) user.GrantSuperAdmin();
        if (request.HomeCompanyId.HasValue) user.SetHomeCompany(request.HomeCompanyId.Value);

        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Users (Id, Username, PasswordHash, DisplayName, IsActive, HomeCompanyId, IsSuperAdmin, CreatedBy, CreatedAt)
                VALUES (@Id, @Username, @PasswordHash, @DisplayName, @IsActive, @HomeCompanyId, @IsSuperAdmin, @CreatedBy, @CreatedAt)",
                new { user.Id, user.Username, user.PasswordHash, user.DisplayName, user.IsActive, user.HomeCompanyId, user.IsSuperAdmin, CreatedBy = _currentUserService.UserId, CreatedAt = ChinaTime.Now });

            if (request.RoleIds?.Any() == true)
            {
                foreach (var roleId in request.RoleIds)
                    await conn.ExecuteAsync("INSERT INTO UserRoles (Id, UserId, RoleId, CreatedBy, CreatedAt) VALUES (@Id, @UserId, @RoleId, @CreatedBy, @CreatedAt)",
                        new { Id = Guid.NewGuid(), UserId = user.Id, RoleId = roleId, CreatedBy = _currentUserService.UserId, CreatedAt = ChinaTime.Now }, tx);
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }

        return (await GetByIdAsync(user.Id, ct))!;
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var user = await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id=@Id", new { Id = id })
            ?? throw new KeyNotFoundException("用户不存在");

        if (request.DisplayName != null || request.Phone != null || request.Email != null)
            user.UpdateProfile(request.DisplayName ?? user.DisplayName, request.Phone, request.Email);
        if (!string.IsNullOrEmpty(request.Password)) user.ChangePassword(request.Password);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) user.Activate(); else user.Deactivate(); }
        if (request.HomeCompanyId.HasValue && request.HomeCompanyId.Value != user.HomeCompanyId) user.SetHomeCompany(request.HomeCompanyId.Value);
        if (request.IsSuperAdmin.HasValue && request.IsSuperAdmin.Value != user.IsSuperAdmin) { if (request.IsSuperAdmin.Value) user.GrantSuperAdmin(); else user.RevokeSuperAdmin(); }

        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync("UPDATE Users SET DisplayName=@DisplayName,PasswordHash=@PasswordHash,Phone=@Phone,Email=@Email,IsActive=@IsActive,HomeCompanyId=@HomeCompanyId,IsSuperAdmin=@IsSuperAdmin,UpdatedBy=@UpdatedBy,UpdatedAt=@UpdatedAt WHERE Id=@Id",
                new { user.DisplayName, user.PasswordHash, Phone = (string?)null, Email = (string?)null, user.IsActive, user.HomeCompanyId, user.IsSuperAdmin, UpdatedBy = _currentUserService.UserId, UpdatedAt = ChinaTime.Now, Id = id }, tx);

            if (request.RoleIds != null)
            {
                await conn.ExecuteAsync("DELETE FROM UserRoles WHERE UserId=@UserId", new { UserId = id }, tx);
                foreach (var roleId in request.RoleIds)
                    await conn.ExecuteAsync("INSERT INTO UserRoles (Id, UserId, RoleId, CreatedBy, CreatedAt) VALUES (@Id, @UserId, @RoleId, @CreatedBy, @CreatedAt)",
                        new { Id = Guid.NewGuid(), UserId = id, RoleId = roleId, CreatedBy = _currentUserService.UserId, CreatedAt = ChinaTime.Now }, tx);
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public async Task SetDefaultCompanyAsync(Guid userId, Guid? companyId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("UPDATE Users SET DefaultCompanyId = @CompanyId WHERE Id = @Id", new { Id = userId, CompanyId = companyId });
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("UPDATE Users SET IsActive = 0 WHERE Id = @Id", new { Id = id });
    }

    private async Task<UserDto> MapToDtoAsync(User user, IDbConnection conn, CancellationToken ct)
    {
        var dto = new UserDto
        {
            Id = user.Id, Username = user.Username, DisplayName = user.DisplayName,
            Phone = user.Phone, Email = user.Email, IsActive = user.IsActive,
            HomeCompanyId = user.HomeCompanyId, IsSuperAdmin = user.IsSuperAdmin,
            CreatedAt = user.CreatedAt, RoleIds = new List<Guid>(), RoleNames = new List<string>()
        };

        var roles = (await conn.QueryAsync("SELECT ur.RoleId, r.Name FROM UserRoles ur LEFT JOIN Roles r ON r.Id = ur.RoleId WHERE ur.UserId = @UserId", new { UserId = user.Id })).ToList();
        foreach (var row in roles)
        {
            dto.RoleIds.Add((Guid)row.RoleId);
            if (!string.IsNullOrEmpty(row.Name))
                dto.RoleNames.Add((string)row.Name);
        }

        if (user.HomeCompanyId.HasValue)
        {
            dto.HomeCompanyName = await conn.QuerySingleOrDefaultAsync<string>("SELECT Name FROM Companies WHERE Id = @Id", new { Id = user.HomeCompanyId.Value });
        }

        return dto;
    }
}
