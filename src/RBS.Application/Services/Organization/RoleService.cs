using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

/// <summary>
/// 角色管理应用服务
/// </summary>
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _uow;

    public RoleService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<RoleDto>> GetListAsync(CancellationToken ct = default)
    {
        var roles = await _uow.Roles.GetAllAsync(ct);
        return roles.Select(MapToDto).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _uow.Roles.GetByIdWithRoleMenusAsync(id, ct);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var role = new Role(request.Name, request.Code);
        role.SetDescription(request.Description);

        // 分配菜单权限
        if (request.MenuIds?.Count > 0)
        {
            foreach (var menuId in request.MenuIds)
                role.RoleMenus.Add(new RoleMenu(role.Id, menuId));
        }

        await _uow.Roles.AddAsync(role, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(role);
    }

    public async Task UpdateAsync(Guid id, CreateRoleRequest request, CancellationToken ct = default)
    {
        var role = await _uow.Roles.GetByIdWithRoleMenusAsync(id, ct)
            ?? throw new KeyNotFoundException("角色不存在");

        role.Rename(request.Name);
        role.SetCode(request.Code);
        role.SetDescription(request.Description);

        // 更新菜单权限
        if (request.MenuIds != null)
        {
            role.RoleMenus.Clear();
            foreach (var menuId in request.MenuIds)
                role.RoleMenus.Add(new RoleMenu(role.Id, menuId));
        }

        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _uow.Roles.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("角色不存在");
        await _uow.Roles.DeleteAsync(role, ct);
        await _uow.CommitAsync(ct);
    }

    public async Task<List<Guid>> GetMenuIdsAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _uow.Roles.GetByIdWithRoleMenusAsync(id, ct);
        return role?.RoleMenus.Select(rm => rm.MenuId).ToList() ?? new();
    }

    public async Task SaveMenuIdsAsync(Guid id, List<Guid> menuIds, CancellationToken ct = default)
    {
        var role = await _uow.Roles.GetByIdWithRoleMenusAsync(id, ct)
            ?? throw new KeyNotFoundException("角色不存在");

        role.RoleMenus.Clear();
        foreach (var menuId in menuIds)
            role.RoleMenus.Add(new RoleMenu(role.Id, menuId));

        await _uow.CommitAsync(ct);
    }

    private static RoleDto MapToDto(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Code = role.Code,
        Description = role.Description,
        IsActive = role.IsActive,
        MenuIds = role.RoleMenus?.Select(rm => rm.MenuId).ToList() ?? new()
    };
}
