using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalsController : ControllerBase
{
    private readonly IJournalAppService _journalAppService;
    private readonly ITenantService _tenant;
    private readonly IApprovalService _approvalService;
    private readonly IReceivableGenerationService _receivableGen;

    public JournalsController(IJournalAppService journalAppService,
        ITenantService tenant, IApprovalService approvalService,
        IReceivableGenerationService receivableGen)
    {
        _journalAppService = journalAppService;
        _tenant = tenant;
        _approvalService = approvalService;
        _receivableGen = receivableGen;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? period,
        [FromQuery] string? billMonth,
        [FromQuery] string? contractNo,
        [FromQuery] Guid? feeCodeId,
        [FromQuery] bool? glPosted,
        [FromQuery] bool? isBilled,
        [FromQuery] Guid? contractId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var companyId = _tenant.EffectiveCompanyId;
        var result = await _journalAppService.GetPagedAsync(companyId, period, billMonth, contractNo, feeCodeId, glPosted, isBilled, contractId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var item = await _journalAppService.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("bycontract")]
    public async Task<IActionResult> GetByContract([FromQuery] Guid contractId, CancellationToken ct)
    {
        var items = await _journalAppService.GetByContractAsync(contractId);
        return Ok(items);
    }

    /// <summary>预览生成应收 — 计算哪些账期缺少 Journal</summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] PreviewRequest request, CancellationToken ct)
    {
        var result = await _journalAppService.PreviewAsync(request.ContractId);
        return Ok(result);
    }

    /// <summary>提交生成应收 — 直接创建或走审批</summary>
    [HttpPost("generaterequest")]
    public async Task<IActionResult> GenerateRequest([FromBody] PreviewRequest request, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _journalAppService.GenerateRequestAsync(request.ContractId, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { code = "PENDING_APPROVAL_EXISTS", message = ex.Message });
        }
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        return Ok(new { message = "出账任务已触发" });
    }

    [HttpPost("post")]
    public async Task<IActionResult> Post([FromBody] List<Guid> ids, CancellationToken ct)
    {
        var result = await _journalAppService.PostAsync(ids);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }
}

public class PreviewRequest
{
    public Guid ContractId { get; set; }
}
