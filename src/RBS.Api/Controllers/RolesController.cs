using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    public RolesController(IRoleService roleService) => _roleService = roleService;

    /// <summary>获取角色列表</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _roleService.GetListAsync(ct);
        return Ok(result);
    }

    /// <summary>获取角色详情（含菜单权限）</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _roleService.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>创建角色</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var result = await _roleService.CreateAsync(request, ct);
        return Ok(result);
    }

    /// <summary>更新角色</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        await _roleService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>删除角色</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _roleService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>获取角色的菜单权限 ID 列表</summary>
    [HttpGet("{id}/menus")]
    public async Task<IActionResult> GetMenus(Guid id, CancellationToken ct)
    {
        var menuIds = await _roleService.GetMenuIdsAsync(id, ct);
        return Ok(menuIds);
    }

    /// <summary>保存角色的菜单权限</summary>
    [HttpPost("{id}/menus")]
    public async Task<IActionResult> SaveMenus(Guid id, [FromBody] List<Guid> menuIds, CancellationToken ct)
    {
        await _roleService.SaveMenuIdsAsync(id, menuIds, ct);
        return NoContent();
    }
}
