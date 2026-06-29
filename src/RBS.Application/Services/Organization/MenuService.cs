using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

/// <summary>
/// 菜单管理应用服务
/// </summary>
public class MenuService : IMenuService
{
    private readonly IUnitOfWork _uow;

    public MenuService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<MenuDto>> GetTreeAsync(CancellationToken ct = default)
    {
        var menus = await _uow.Menus.GetTreeAsync(ct);
        var dtos = menus.Select(MapToDto).ToList();
        return BuildTree(dtos, null);
    }

    public async Task<MenuDto> CreateAsync(CreateMenuRequest request, CancellationToken ct = default)
    {
        var menu = new Menu(request.Name);
        ApplyRequest(menu, request);
        await _uow.Menus.AddAsync(menu, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(menu);
    }

    public async Task UpdateAsync(Guid id, CreateMenuRequest request, CancellationToken ct = default)
    {
        var menu = await _uow.Menus.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("菜单不存在");
        menu.Rename(request.Name);
        ApplyRequest(menu, request);
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var menu = await _uow.Menus.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("菜单不存在");
        await _uow.Menus.DeleteAsync(menu, ct);
        await _uow.CommitAsync(ct);
    }

    private static void ApplyRequest(Menu menu, CreateMenuRequest request)
    {
        menu.SetPermissionCode(request.PermissionCode);
        menu.SetPath(request.Path);
        menu.SetIcon(request.Icon);
        menu.SetParentId(request.ParentId);
        menu.SetSortOrder(request.SortOrder);
        menu.SetScope(request.Scope);
    }

    internal static MenuDto MapToDto(Menu menu) => new()
    {
        Id = menu.Id,
        Name = menu.Name,
        PermissionCode = menu.PermissionCode,
        Path = menu.Path,
        Icon = menu.Icon,
        ParentId = menu.ParentId,
        SortOrder = menu.SortOrder,
        IsActive = menu.IsActive,
        Scope = menu.Scope
    };

    private static List<MenuDto> BuildTree(List<MenuDto> flatList, Guid? parentId)
    {
        return flatList
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.SortOrder)
            .Select(m =>
            {
                m.Children = BuildTree(flatList, m.Id);
                return m;
            })
            .ToList();
    }
}
