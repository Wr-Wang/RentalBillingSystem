using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Common;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceivablesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IReceivableGenerationService _generationService;
    private readonly IApprovalService _approvalService;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ICurrentUserService _currentUser;

    public ReceivablesController(IUnitOfWork uow, IReceivableGenerationService generationService,
        IApprovalService approvalService, IDbConnectionFactory db, ISqlLoader sql,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _generationService = generationService;
        _approvalService = approvalService;
        _db = db;
        _sql = sql;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        if (contractId == null) return Ok(new List<object>());
        var list = await _uow.ReceivablePlans.GetByContractIdAsync(contractId.Value, ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _uow.ReceivablePlans.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpGet("byfee")]
    public async Task<IActionResult> GetByContractAndFee([FromQuery] Guid contractId, [FromQuery] Guid feeCodeId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync(
            _sql.Get("Receivable.Select.Plan.ByContractAndFee"),
            new { ContractId = contractId, FeeCodeId = feeCodeId });
        // ★ DapperRow 序列化为 JSON 时键名为 PascalCase（字典不受 CamelCase 策略影响）
        //    需映射为匿名对象以确保前端接收到 camelCase 属性名
        var plans = rows.Select(r => new
        {
            id = (Guid)r.Id,
            period = (string)r.Period,
            amount = (decimal)r.Amount,
            dueDate = r.DueDate is DateOnly dd ? dd.ToString("yyyy-MM-dd") : ((DateTime)r.DueDate).ToString("yyyy-MM-dd"),
            status = (string)r.Status,
            received = (decimal)r.Received,
            lateFee = (decimal)r.LateFee,
            isBilled = (bool)r.IsBilled
        }).ToList();
        return Ok(plans);
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] GenerateReceivablesRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty)
            return BadRequest(new { message = "contractId 不能为空" });

        var contract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct);
        if (contract == null) return NotFound(new { message = "合同不存在" });
        if (contract.Status != "Active")
            return BadRequest(new { message = "合同状态非生效中" });

        var allPeriods = _generationService.SplitPeriods(contract);
        var from = request.PeriodFrom ?? allPeriods.First();
        var to = request.PeriodTo ?? allPeriods.Last();

        using var conn = _db.CreateConnection(); conn.Open();
        var items = new List<object>();
        decimal totalAmount = 0;

        foreach (var period in allPeriods.Where(p => string.Compare(p, from, StringComparison.Ordinal) >= 0
            && string.Compare(p, to, StringComparison.Ordinal) <= 0))
        {
            var lastDay = DateTime.DaysInMonth(int.Parse(period[..4]), int.Parse(period[5..7]));
            var fees = await conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                new { ContractId = request.ContractId, PeriodStart = $"{period}-01", PeriodEnd = $"{period}-{lastDay:D2}" });

            foreach (var f in fees)
            {
                items.Add(new { period, feeCodeId = (Guid)f.FeeCodeId, feeName = (string)f.FeeName,
                    amount = (decimal)f.Amount, dueDate = _generationService.CalculateDueDate(period, contract) });
                totalAmount += (decimal)f.Amount;
            }
        }

        var matchedPeriods = allPeriods.Where(p => string.Compare(p, from, StringComparison.Ordinal) >= 0
            && string.Compare(p, to, StringComparison.Ordinal) <= 0).ToList();
        return Ok(new { periods = matchedPeriods, totalAmount, items,
            summary = $"将生成 {items.Count} 条应收记录，合计 ¥{totalAmount:N2}" });
    }

    [HttpPost("generaterequest")]
    public async Task<IActionResult> GenerateRequest([FromBody] GenerateReceivablesRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty)
            return BadRequest(new { message = "contractId 不能为空" });

        var contract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct);
        if (contract == null) return NotFound(new { message = "合同不存在" });
        if (contract.Status != "Active")
            return BadRequest(new { message = "合同状态非生效中" });

        var userId = _currentUser.UserId;
        var allPeriods = _generationService.SplitPeriods(contract);
        var from = request.PeriodFrom ?? allPeriods.First();
        var to = request.PeriodTo ?? allPeriods.Last();
        var now = ChinaTime.Now;

        var genReq = new ReceivableGenerateRequest(request.ContractId, contract.CompanyId, from, to);
        genReq.SetCreated(userId, now, null, null);
        await _uow.ReceivableGenerateRequests.AddAsync(genReq, ct);

        using var conn = _db.CreateConnection(); conn.Open();
        foreach (var period in allPeriods.Where(p => string.Compare(p, from, StringComparison.Ordinal) >= 0
            && string.Compare(p, to, StringComparison.Ordinal) <= 0))
        {
            var lastDay = DateTime.DaysInMonth(int.Parse(period[..4]), int.Parse(period[5..7]));
            var fees = await conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                new { ContractId = request.ContractId, PeriodStart = $"{period}-01", PeriodEnd = $"{period}-{lastDay:D2}" });

            foreach (var f in fees)
            {
                var item = new ReceivableGenerateRequestItem(genReq.Id, (Guid)f.FeeCodeId, (string)f.FeeName,
                    period, (decimal)f.Amount, _generationService.CalculateDueDate(period, contract));
                await _uow.ReceivableGenerateRequestItems.AddAsync(item, ct);
            }
        }
        await _uow.CommitAsync(ct);

        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_GENERATE_RECEIVABLE", ct);
        if (approvalType == null)
        {
            try { await _generationService.GenerateAsync(request.ContractId, from, to, ct); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            return Ok(new { message = "应收已成功生成" });
        }

        genReq.Submit(); await _uow.CommitAsync(ct);
        var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
        {
            ApprovalTypeId = approvalType.Id,
            Title = $"[生成应收] {contract.ContractNo}",
            Description = $"{from} ~ {to}",
            TargetEntityId = genReq.Id,
            TargetEntityType = "ReceivableGeneration"
        }, ct);
        return Ok(new { status = "PendingApproval", requestId = genReq.Id, approvalRequestId = approvalResult.Id });
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateReceivablesRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty)
            return BadRequest(new { message = "contractId 不能为空" });
        try
        {
            var count = await _generationService.GenerateAsync(request.ContractId, request.PeriodFrom, request.PeriodTo, ct);
            return Ok(new { message = $"应收已成功生成，共 {count} 条", totalCreated = count });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}

public class GenerateReceivablesRequest
{
    public Guid ContractId { get; set; }
    public string? PeriodFrom { get; set; }
    public string? PeriodTo { get; set; }
}
