using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeterReadingsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    public MeterReadingsController(IUnitOfWork uow, IDbConnectionFactory db) { _uow = uow; _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractFeeConfigId, CancellationToken ct)
    {
        if (contractFeeConfigId == null) return Ok(new List<object>());
        var list = await _uow.MeterReadings.GetHistoryAsync(contractFeeConfigId.Value,
            RBS.Core.Common.ChinaTime.Now.Year, RBS.Core.Common.ChinaTime.Now.Month, ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MeterReading dto, CancellationToken ct)
    {
        await _uow.MeterReadings.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MeterReading dto, CancellationToken ct)
    {
        var entity = await _uow.MeterReadings.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var entity = await _uow.MeterReadings.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        if (entity.Status == "Draft")
        {
            // DapperRepository 不支持属性赋值，使用 SQL 更新
            await _db.CreateConnection().ExecuteAsync(
                "UPDATE MeterReadings SET Status='Confirmed' WHERE Id=@Id AND Status='Draft'", new { Id = id });
        }
        return Ok(new { id, status = "Confirmed" });
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] List<MeterReading> list, CancellationToken ct)
    {
        if (list.Count == 0) return Ok(new { imported = 0 });

        int imported = 0;
        foreach (var item in list)
        {
            // 去重：同一配置+年月只能有一条
            var exists = await _uow.MeterReadings.ReadingExistsAsync(
                item.ContractFeeConfigId, item.Year, item.Month, ct);
            if (exists) continue;

            await _uow.MeterReadings.AddAsync(item, ct);
            imported++;
        }

        await _uow.CommitAsync(ct);
        return Ok(new { imported, total = list.Count });
    }
}
