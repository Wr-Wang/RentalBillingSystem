using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IRenewalService _renewalService;
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    public ContractsController(IContractService contractService, IRenewalService renewalService, IUnitOfWork uow, IDbConnectionFactory db)
    {
        _contractService = contractService;
        _renewalService = renewalService;
        _uow = uow;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] Guid? companyId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null, [FromQuery] string? status = null)
    {
        if (companyId == null) return Ok(new { items = new List<object>(), total = 0, page = 1, pageSize = 10, totalPages = 0 });
        var result = await _contractService.GetPagedListAsync(companyId.Value, page, pageSize, keyword, status, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var dto = await _contractService.GetByIdAsync(id, ct);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RBS.Core.Entities.Contract.Contract dto, CancellationToken ct)
    {
        await _uow.Contracts.AddAsync(dto, ct);
        await _uow.CommitAsync(ct);
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RBS.Core.Entities.Contract.Contract dto, CancellationToken ct)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        await _uow.CommitAsync(ct);
        return NoContent();
    }

    [HttpPost("{id}/terminate")]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] Dictionary<string, string> body, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        body.TryGetValue("reason", out var reason);
        await conn.ExecuteAsync("UPDATE Contracts SET Status='Terminated',TerminationReason=@Reason WHERE Id=@Id", new { Id = id, Reason = reason ?? "手动终止" });
        return Ok(new { id, status = "Terminated" });
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("UPDATE Contracts SET Status='Suspended' WHERE Id=@Id", new { Id = id });
        return Ok(new { id, status = "Suspended" });
    }

    [HttpPost("{id}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("UPDATE Contracts SET Status='Active' WHERE Id=@Id", new { Id = id });
        return Ok(new { id, status = "Active" });
    }

    // ===== 续签相关 API（新审批流程） =====

    /// <summary>续签预览：检查欠费、并发、展示继承配置</summary>
    [HttpGet("{id}/renewal/preview")]
    public async Task<IActionResult> RenewalPreview(Guid id, CancellationToken ct)
    {
        try
        {
            var preview = await _renewalService.PreviewAsync(id, ct);
            return Ok(preview);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>提交续签审批</summary>
    [HttpPost("{id}/renewal/submit")]
    public async Task<IActionResult> SubmitRenewal(Guid id, [FromBody] RBS.Application.DTOs.Contract.SubmitRenewalRequest request, CancellationToken ct)
    {
        request.ContractId = id;
        try
        {
            var userId = GetCurrentUserId();
            var result = await _renewalService.SubmitAsync(request, userId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>获取续签历史</summary>
    [HttpGet("{id}/renewal/history")]
    public async Task<IActionResult> GetRenewalHistory(Guid id, CancellationToken ct)
    {
        var history = await _renewalService.GetHistoryAsync(id, ct);
        return Ok(history);
    }

    /// <summary>获取续签链</summary>
    [HttpGet("{id}/renewal/chain")]
    public async Task<IActionResult> GetRenewalChain(Guid id, CancellationToken ct)
    {
        var chain = await _renewalService.GetRenewalChainAsync(id, ct);
        return Ok(chain);
    }

    /// <summary>获取合同允许的操作</summary>
    [HttpGet("{id}/allowed-operations")]
    public async Task<IActionResult> GetAllowedOperations(Guid id, CancellationToken ct)
    {
        try
        {
            var ops = await _renewalService.GetAllowedOperationsAsync(id, ct);
            return Ok(ops);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private Guid GetCurrentUserId()
    {
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }

    [HttpGet("{id}/timeline")]
    public IActionResult GetTimeline(Guid id) => Ok(new List<object>());
}
