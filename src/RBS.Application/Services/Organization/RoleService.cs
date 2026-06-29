using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;

    public RoleService(IUnitOfWork uow, ITenantService tenant)
    {
        _uow = uow;
        _tenant = tenant;
    }

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

        if (request.MenuIds?.Count > 0)
        {
            var allowedIds = await FilterSystemMenusAsync(request.MenuIds, ct);
            foreach (var menuId in allowedIds)
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

        if (request.MenuIds != null)
        {
            var allowedIds = await FilterSystemMenusAsync(request.MenuIds, ct);
            role.RoleMenus.Clear();
            foreach (var menuId in allowedIds)
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

        var allowedIds = await FilterSystemMenusAsync(menuIds, ct);
        role.RoleMenus.Clear();
        foreach (var menuId in allowedIds)
            role.RoleMenus.Add(new RoleMenu(role.Id, menuId));

        await _uow.CommitAsync(ct);
    }

    private async Task<List<Guid>> FilterSystemMenusAsync(List<Guid> menuIds, CancellationToken ct)
    {
        if (_tenant.IsSuperAdmin) return menuIds;
        var allMenus = await _uow.Menus.GetAllAsync(ct);
        var systemIds = allMenus.Where(m => m.Scope == "System").Select(m => m.Id).ToHashSet();
        return menuIds.Where(id => !systemIds.Contains(id)).ToList();
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
