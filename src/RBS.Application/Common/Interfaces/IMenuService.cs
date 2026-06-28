using RBS.Application.DTOs.Organization;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 菜单管理应用服务
/// </summary>
public interface IMenuService
{
    Task<List<MenuDto>> GetTreeAsync(CancellationToken ct = default);
    Task<MenuDto> CreateAsync(CreateMenuRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CreateMenuRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
