using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBS.Infrastructure.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/contract-fee-configs")]
[Authorize]
public class ContractFeeConfigsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ContractFeeConfigsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        if (contractId == null) return Ok(new List<object>());
        var list = await _db.Set<RBS.Core.Entities.Contract.ContractFeeConfig>()
            .Where(f => f.ContractId == contractId.Value)
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] object dto, CancellationToken ct)
    {
        return NoContent();
    }
}
