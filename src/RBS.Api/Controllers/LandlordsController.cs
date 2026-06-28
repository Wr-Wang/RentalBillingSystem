using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LandlordsController : ControllerBase
{
    private readonly ILandlordService _landlordService;

    public LandlordsController(ILandlordService landlordService) => _landlordService = landlordService;

    /// <summary>分页查询房东列表</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = new LandlordQuery
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            IsActive = isActive
        };
        var result = await _landlordService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>获取房东详情</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _landlordService.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>创建房东</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLandlordRequest request, CancellationToken ct)
    {
        var result = await _landlordService.CreateAsync(request, ct);
        return Ok(result);
    }

    /// <summary>更新房东信息</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateLandlordRequest request, CancellationToken ct)
    {
        await _landlordService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>删除（停用）房东</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _landlordService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>获取房东资产概况</summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetStats(Guid id, CancellationToken ct)
    {
        var result = await _landlordService.GetStatsAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
