using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Application.Services.Billing;
using RBS.Application.Services.Contract;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalsController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ITenantService _tenant;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IApprovalService _approvalService;
    private readonly IReceivableGenerationService _receivableGen;

    public JournalsController(IDbConnectionFactory db, ISqlLoader sql, ITenantService tenant,
        IUnitOfWork uow, ICurrentUserService currentUser,
        IApprovalService approvalService,
        IReceivableGenerationService receivableGen)
    {
        _db = db;
        _sql = sql;
        _tenant = tenant;
        _uow = uow;
        _currentUser = currentUser;
        _approvalService = approvalService;
        _receivableGen = receivableGen;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? period,
        [FromQuery] string? contractNo,
        [FromQuery] Guid? feeCodeId,
        [FromQuery] bool? glPosted,
        [FromQuery] Guid? contractId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var companyId = _tenant.EffectiveCompanyId;
        if (companyId == null) return Ok(new { items = new List<object>(), total = 0 });

        using var conn = _db.CreateConnection();
        conn.Open();
        var items = await conn.QueryAsync(_sql.Get("Billing.Select.Journal.Paged"),
            new { CoId = companyId, Period = period, CNo = $"%{contractNo}%", FId = feeCodeId, GLP = glPosted, CId = contractId, Offset = (page - 1) * pageSize, PageSize = pageSize });
        var total = await conn.QuerySingleAsync<int>(_sql.Get("Billing.Select.Journal.PagedCount"),
            new { CoId = companyId, Period = period, CNo = $"%{contractNo}%", FId = feeCodeId, GLP = glPosted, CId = contractId });
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var item = await conn.QuerySingleOrDefaultAsync(
            _sql.Get("Billing.Select.Journal.ById"), new { Id = id });
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("bycontract")]
    public async Task<IActionResult> GetByContract([FromQuery] Guid contractId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var items = await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.ByContractWithPayment"),
            new { CId = contractId });
        return Ok(items);
    }

    /// <summary>预览生成应收 — 计算哪些账期缺少 Journal</summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] PreviewRequest request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct);
        if (contract == null) return NotFound(new { message = "合同不存在" });
        if (contract.Status != "Active")
            return BadRequest(new { message = "只有生效中的合同才能生成应收" });

        // 取合同有效期内的所有账期
        var allPeriods = ReceivableGenerationService.SplitPeriodsStatic(contract);
        // 过滤已存在的 Journal
        using var conn = _db.CreateConnection();
        conn.Open();
        var existing = (await conn.QueryAsync<string>(
            "SELECT DISTINCT Period FROM Journals WHERE ContractId = @CId", new { CId = request.ContractId })).ToHashSet();
        var missing = allPeriods.Where(p => !existing.Contains(p)).ToList();

        // 估算每个缺漏账期的金额
        var items = new List<object>();
        foreach (var period in missing)
        {
            var lastDay = DateTime.DaysInMonth(int.Parse(period[..4]), int.Parse(period[5..7]));
            var dueDay = contract.EndDate.HasValue ? Math.Min(contract.EndDate.Value.Day, lastDay) : lastDay;
            decimal totalAmt = 0;
            var feeConfigs = contract.FeeConfigs?.Where(f => IsFeeEffectiveForPeriod(f, period)).ToList() ?? new();
            foreach (var fc in feeConfigs) totalAmt += fc.Amount;
            items.Add(new { period, dueDate = $"{period}-{dueDay:D2}", amount = totalAmt, feeCount = feeConfigs.Count });
        }

        return Ok(new { items, totalAmount = items.Sum(i => (decimal)((dynamic)i).amount), missingCount = missing.Count });
    }

    /// <summary>提交生成应收 — 直接创建或走审批</summary>
    [HttpPost("generaterequest")]
    public async Task<IActionResult> GenerateRequest([FromBody] PreviewRequest request, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct);
        if (contract == null) return NotFound(new { message = "合同不存在" });

        // 检查是否有应收生成审批类型
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("RECEIVABLE_GENERATE", ct);
        if (approvalType != null && contract.Status == "Active")
        {
            // 走审批流
            var userId = _currentUser.UserId;
            var approvalResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
            {
                ApprovalTypeId = approvalType.Id,
                Title = $"[应收生成] {contract.ContractNo}",
                Description = $"手动触发生成应收",
                TargetEntityId = request.ContractId,
                TargetEntityType = "ReceivableGeneration"
            }, ct);
            await _uow.CommitAsync(ct);
            return Ok(new { status = "PendingApproval", id = approvalResult.Id, message = "应收生成请求已提交审批" });
        }

        // 无审批配置或 Draft 合同 → 直接执行
        var created = await _receivableGen.GenerateAsync(request.ContractId, null, null, ct);
        if (created > 0)
        {
            // 更新 GL（追加式快照）
            var now = ChinaTime.Now;
            var period = $"{now.Year}-{now.Month:D2}";
            using var conn = _db.CreateConnection();
            conn.Open();
            var latest = await conn.QuerySingleOrDefaultAsync(
                _sql.Get("Accounting.Select.GL.LatestByPeriod"),
                new { CoId = contract.CompanyId, Period = period });
            var prevBilled = latest != null ? (decimal)((dynamic)latest).TotalBilled : 0m;
            var prevReceived = latest != null ? (decimal)((dynamic)latest).TotalReceived : 0m;
            var opening = latest != null ? (decimal)((dynamic)latest).OpeningBalance : 0m;
            // 需要汇总本次新增的金额
            var sumAmt = 0m; using var c2 = _db.CreateConnection(); c2.Open();
            sumAmt = await c2.QuerySingleAsync<decimal>(
                "SELECT ISNULL(SUM(Amount),0) FROM Journals WHERE ContractId=@CId AND Period=@P AND CreatedAt>=DATEADD(MINUTE,-1,GETUTCDATE())",
                new { CId = request.ContractId, P = period });
            var newBilled = prevBilled + sumAmt;
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Default"),
                new { Id = Guid.NewGuid(), CoId = contract.CompanyId, Period = period,
                    Opening = opening, Billed = newBilled, Received = prevReceived,
                    Closing = opening + newBilled - prevReceived, CBy = Guid.Empty });
        }
        return Ok(new { message = $"已生成 {created} 条应收记录", count = created });
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        return Ok(new { message = "出账任务已触发" });
    }

    private static bool IsFeeEffectiveForPeriod(Core.Entities.Contract.ContractFeeConfig feeConfig, string period)
    {
        var periodStart = DateOnly.Parse($"{period}-01");
        var daysInMonth = DateTime.DaysInMonth(periodStart.Year, periodStart.Month);
        var periodEnd = periodStart.AddDays(daysInMonth - 1);
        if (feeConfig.EffectiveDate != null)
        {
            var eff = DateOnly.Parse(feeConfig.EffectiveDate);
            if (periodEnd < eff) return false;
        }
        if (feeConfig.ExpiryDate != null)
        {
            var exp = DateOnly.Parse(feeConfig.ExpiryDate);
            if (periodStart > exp) return false;
        }
        return true;
    }
}

public class PreviewRequest
{
    public Guid ContractId { get; set; }
}
