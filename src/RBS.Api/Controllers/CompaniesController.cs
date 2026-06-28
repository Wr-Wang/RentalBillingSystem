using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService) => _companyService = companyService;

    /// <summary>分页查询公司列表</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = new CompanyQuery
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            IsActive = isActive
        };
        var result = await _companyService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>获取公司详情</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _companyService.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>创建公司</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        var result = await _companyService.CreateAsync(request, ct);
        return Ok(result);
    }

    /// <summary>更新公司信息</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        await _companyService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>删除（停用）公司</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _companyService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>获取公司资产概况</summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetStats(Guid id, CancellationToken ct)
    {
        var result = await _companyService.GetStatsAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
