using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HolidayCalendarsController : ControllerBase
{
    private readonly IHolidayCalendarService _service;

    public HolidayCalendarsController(IHolidayCalendarService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? year, CancellationToken ct)
    {
        var result = await _service.GetByYearAsync(year ?? DateTime.Now.Year, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHolidayCalendarRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHolidayCalendarRequest request, CancellationToken ct)
    {
        await _service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>从 timor.tech API 导入全年节假日数据</summary>
    [HttpPost("import/{year}")]
    public async Task<IActionResult> Import(int year, CancellationToken ct)
    {
        var result = await _service.ImportYearDataAsync(year, ct);
        return Ok(new
        {
            result.ImportedCount,
            result.SkippedCount,
            result.Imported,
            result.Skipped,
            message = $"新增 {result.ImportedCount} 条，跳过 {result.SkippedCount} 条"
        });
    }
}
