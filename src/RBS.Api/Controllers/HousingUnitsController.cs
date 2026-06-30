using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces; using RBS.Application.DTOs.Property;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize]
public class HousingUnitsController : ControllerBase
{
    private readonly IHousingUnitService _service;
    private readonly ITenantService _ts;
    public HousingUnitsController(IHousingUnitService service, ITenantService ts) { _service = service; _ts = ts; }

    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] string? buildingName, [FromQuery] string? keyword, [FromQuery] string? status, CancellationToken ct)
        => Ok(await _service.GetListAsync(buildingName, keyword, status, ct));

    [HttpGet("tree")] public async Task<IActionResult> GetTree([FromQuery] Guid? companyId, CancellationToken ct)
    { var cid = companyId ?? _ts.EffectiveCompanyId ?? Guid.Empty; return Ok(await _service.GetTreeAsync(cid, ct)); }

    [HttpGet("stats")] public async Task<IActionResult> GetStats(CancellationToken ct) => Ok(await _service.GetStatsAsync(ct));

    [HttpGet("buildinglist")] public async Task<IActionResult> GetBuildingList(CancellationToken ct)
    {
        var all = await _service.GetListAsync(ct: ct);
        var list = all.Where(u => !string.IsNullOrEmpty(u.BuildingName))
            .GroupBy(u => new { u.BuildingName, u.CompanyId })
            .Select(g => new { g.Key.BuildingName, g.First().BuildingCode, g.First().BuildingAddress, g.Key.CompanyId })
            .OrderBy(b => b.BuildingName).ToList();
        return Ok(list);
    }

    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    { var r = await _service.GetByIdAsync(id, ct); if (r == null) return NotFound(); return Ok(r); }

    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateHousingUnitRequest r, CancellationToken ct) => Ok(await _service.CreateAsync(r, ct));

    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHousingUnitRequest r, CancellationToken ct)
    { await _service.UpdateAsync(id, r, ct); return NoContent(); }

    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { await _service.DeleteAsync(id, ct); return NoContent(); }
}
