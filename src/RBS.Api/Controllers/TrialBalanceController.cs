using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBS.Infrastructure.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/trial-balance")]
[Authorize]
public class TrialBalanceController : ControllerBase
{
    private readonly AppDbContext _db;
    public TrialBalanceController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly? endDate, CancellationToken ct)
    {
        return Ok(new List<object>());
    }
}
