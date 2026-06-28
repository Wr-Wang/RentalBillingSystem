using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;
    public MenusController(IMenuService menuService) => _menuService = menuService;

    /// <summary>获取菜单树（平铺列表，前端自行构建树）</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _menuService.GetTreeAsync(ct);
        return Ok(result);
    }

    /// <summary>创建菜单</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request, CancellationToken ct)
    {
        var result = await _menuService.CreateAsync(request, ct);
        return Ok(result);
    }

    /// <summary>更新菜单</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateMenuRequest request, CancellationToken ct)
    {
        await _menuService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>删除菜单</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _menuService.DeleteAsync(id, ct);
        return NoContent();
    }
}
