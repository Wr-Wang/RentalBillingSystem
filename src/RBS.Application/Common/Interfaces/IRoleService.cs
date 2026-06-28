using RBS.Application.DTOs.Organization;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 角色管理应用服务
/// </summary>
public interface IRoleService
{
    Task<List<RoleDto>> GetListAsync(CancellationToken ct = default);
    Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CreateRoleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<Guid>> GetMenuIdsAsync(Guid id, CancellationToken ct = default);
    Task SaveMenuIdsAsync(Guid id, List<Guid> menuIds, CancellationToken ct = default);
}
