using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

/// <summary>
/// 用户管理应用服务实现
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<UserDto>> GetListAsync(CancellationToken ct = default)
    {
        var users = await _uow.Users.GetAllWithRolesAsync(ct);
        var dtos = new List<UserDto>();
        foreach (var user in users)
            dtos.Add(await MapToDtoAsync(user, ct));
        return dtos;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdWithRolesAsync(id, ct);
        return user == null ? null : await MapToDtoAsync(user, ct);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var unique = await _uow.Users.IsUsernameUniqueAsync(request.Username, null, ct);
        if (!unique) throw new InvalidOperationException($"用户名 '{request.Username}' 已存在");

        var user = new User(request.Username, request.DisplayName, request.Password);
        if (request.IsSuperAdmin) user.GrantSuperAdmin();
        if (request.HomeLandlordId.HasValue) user.SetHomeLandlord(request.HomeLandlordId.Value);

        await _uow.Users.AddAsync(user, ct);

        if (request.RoleIds?.Any() == true)
        {
            foreach (var roleId in request.RoleIds)
                user.AssignRole(roleId);
        }

        await _uow.CommitAsync(ct);
        return await GetByIdAsync(user.Id, ct) ?? throw new InvalidOperationException("创建失败");
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdWithRolesAsync(id, ct);
        if (user == null) throw new KeyNotFoundException("用户不存在");

        if (request.DisplayName != null || request.Phone != null || request.Email != null)
            user.UpdateProfile(request.DisplayName ?? user.DisplayName, request.Phone, request.Email);

        if (!string.IsNullOrEmpty(request.Password))
            user.ChangePassword(request.Password);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) user.Activate();
            else user.Deactivate();
        }

        if (request.HomeLandlordId.HasValue && request.HomeLandlordId.Value != user.HomeLandlordId)
            user.SetHomeLandlord(request.HomeLandlordId.Value);

        if (request.IsSuperAdmin.HasValue && request.IsSuperAdmin.Value != user.IsSuperAdmin)
        {
            if (request.IsSuperAdmin.Value) user.GrantSuperAdmin();
            else user.RevokeSuperAdmin();
        }

        if (request.RoleIds != null)
        {
            var currentRoleIds = user.Roles.Select(r => r.RoleId).ToHashSet();
            var newRoleIds = request.RoleIds.ToHashSet();
            foreach (var roleId in currentRoleIds.Except(newRoleIds))
                user.RemoveRole(roleId);
            foreach (var roleId in newRoleIds.Except(currentRoleIds))
                user.AssignRole(roleId);
        }

        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user == null) throw new KeyNotFoundException("用户不存在");
        user.Deactivate();
        await _uow.CommitAsync(ct);
    }

    private async Task<UserDto> MapToDtoAsync(User user, CancellationToken ct)
    {
        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Phone = user.Phone,
            Email = user.Email,
            IsActive = user.IsActive,
            HomeLandlordId = user.HomeLandlordId,
            IsSuperAdmin = user.IsSuperAdmin,
            CreatedAt = user.CreatedAt,
            RoleIds = user.Roles.Select(r => r.RoleId).ToList()
        };

        // 获取角色名称
        foreach (var ur in user.Roles)
        {
            var role = await _uow.Roles.GetByIdAsync(ur.RoleId, ct);
            if (role != null) dto.RoleNames.Add(role.Name);
        }

        // 获取房东名称
        if (user.HomeLandlordId.HasValue)
        {
            var landlord = await _uow.Landlords.GetByIdAsync(user.HomeLandlordId.Value, ct);
            if (landlord != null) dto.HomeLandlordName = landlord.Name;
        }

        return dto;
    }
}
