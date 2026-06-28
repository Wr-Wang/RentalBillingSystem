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
        var users = await _uow.Users.GetAllAsync(ct);
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            DisplayName = u.DisplayName,
            Phone = u.Phone,
            Email = u.Email,
            IsActive = u.IsActive,
            HomeLandlordId = u.HomeLandlordId,
            IsSuperAdmin = u.IsSuperAdmin,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user == null) return null;
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Phone = user.Phone,
            Email = user.Email,
            IsActive = user.IsActive,
            HomeLandlordId = user.HomeLandlordId,
            IsSuperAdmin = user.IsSuperAdmin,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var unique = await _uow.Users.IsUsernameUniqueAsync(request.Username, null, ct);
        if (!unique) throw new InvalidOperationException($"用户名 '{request.Username}' 已存在");

        var user = new User(request.Username, request.DisplayName, request.Password);
        // TODO: 使用 BCrypt 加密密码 user.ChangePassword(BCrypt.HashPassword(request.Password));
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
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user == null) throw new KeyNotFoundException("用户不存在");

        if (request.DisplayName != null) user.UpdateProfile(request.DisplayName, request.Phone, request.Email);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) user.Activate(); else user.Deactivate(); }
        if (request.RoleIds != null)
        {
            // 重新分配角色
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
}
