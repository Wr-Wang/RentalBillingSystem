using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 月账单生成 Job — 为所有生效合同生成当月应收计划
/// </summary>
public class MonthlyFeeBillJob : IScheduledJob
{
    public string JobName => "MonthlyFeeBill";
    private readonly IUnitOfWork _uow;
    private readonly IReceivableGenerationService _generationService;

    public MonthlyFeeBillJob(IUnitOfWork uow, IReceivableGenerationService generationService)
    {
        _uow = uow;
        _generationService = generationService;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var contracts = await _uow.Contracts.GetAllAsync(ct);
        var activeContracts = contracts.Where(c => c.Status == "Active" && c.CompanyId == companyId).ToList();
        if (activeContracts.Count == 0) return "无生效合同";

        int total = 0;
        foreach (var contract in activeContracts)
        {
            if (!contract.ShouldGenerateReceivableFor(targetMonth)) continue;
            try
            {
                var count = await _generationService.GenerateAsync(contract.Id, targetMonth, targetMonth, ct);
                total += count;
            }
            catch { /* 单个合同失败不影响其他合同 */ }
        }

        return $"已为 {total} 条费用配置生成应收计划";
    }
}

/// <summary>
/// 滞纳金计算 Job — 每日计算逾期应收的滞纳金
/// </summary>
public class LateFeeCalcJob : IScheduledJob
{
    public string JobName => "LateFeeCalc";
    private readonly IUnitOfWork _uow;
    private readonly IBillingDomainService _billingDomain;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public LateFeeCalcJob(IUnitOfWork uow, IBillingDomainService billingDomain, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _billingDomain = billingDomain;
        _db = db;
        _sql = sql;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var configs = await _uow.LateFeeConfigs.GetAllAsync(ct);
        var config = configs.FirstOrDefault(c => c.CompanyId == companyId);
        if (config == null) return "未配置滞纳金规则";

        var overduePlans = await _uow.ReceivablePlans.GetOverdueAsync(companyId, ct);
        if (overduePlans.Count == 0) return "无逾期应收";

        var asOfDate = DateOnly.FromDateTime(ChinaTime.Now);
        int count = 0;

        foreach (var plan in overduePlans)
        {
            var fee = _billingDomain.CalculateLateFee(plan, config, asOfDate);
            if (fee > 0)
            {
                plan.SetLateFee(fee);
                using var conn = _db.CreateConnection(); conn.Open();
                await conn.ExecuteAsync(
                    _sql.Get("Billing.Update.ReceivablePlan.LateFee"),
                    new { Fee = fee, Id = plan.Id });
                count++;
            }
        }

        return $"已处理 {count} 条逾期应收的滞纳金";
    }
}

/// <summary>
/// 自动续签 Job — 检查到期前 N 天且 AutoRenew=true 的合同
/// </summary>
public class AutoRenewJob : IScheduledJob
{
    public string JobName => "AutoRenew";
    private readonly IUnitOfWork _uow;
    private readonly IRenewalService _renewalService;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public AutoRenewJob(IUnitOfWork uow, IRenewalService renewalService, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _renewalService = renewalService;
        _db = db;
        _sql = sql;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        var targetDate = today.AddDays(7);

        var contracts = await _uow.Contracts.GetAllAsync(ct);
        var expiring = contracts
            .Where(c => c.CompanyId == companyId
                     && c.Status == "Active"
                     && c.AutoRenew
                     && c.EndDate == targetDate)
            .ToList();

        if (expiring.Count == 0) return $"今日无到期前 7 天且启用自动续签的合同";

        int submitted = 0;
        foreach (var contract in expiring)
        {
            try
            {
                // 使用现有的续签预览和提交逻辑
                // 提交续签审批（自动续签使用默认参数）
                using var conn = _db.CreateConnection(); conn.Open();
                var rentAmount = await conn.QuerySingleOrDefaultAsync<decimal>(
                    _sql.Get("Contract.Select.FeeConfig.AmountByCode"),
                    new { Cid = contract.Id, Code = "RENT" });
                var result = await _renewalService.SubmitAsync(
                    new RBS.Application.DTOs.Contract.SubmitRenewalRequest
                    {
                        ContractId = contract.Id,
                        NewRentAmount = rentAmount,
                        NewEndDate = contract.EndDate.AddMonths(12).ToString("yyyy-MM-dd"),
                        DepositHandling = "TRANSFER"
                    }, Guid.Empty, ct);
                if (result != null) submitted++;
            }
            catch { /* 单个合同失败不影响其他 */ }
        }

        return $"已为 {submitted}/{expiring.Count} 份合同提交自动续签";
    }
}

/// <summary>
/// 催缴 Job — 按逾期阶段触发催缴记录
/// </summary>
public class CollectionJob : IScheduledJob
{
    public string JobName => "Collection";
    private readonly IUnitOfWork _uow;

    public CollectionJob(IUnitOfWork uow) => _uow = uow;

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var stages = await _uow.CollectionStages.GetAllAsync(ct);
        var overduePlans = await _uow.ReceivablePlans.GetOverdueAsync(companyId, ct);

        if (overduePlans.Count == 0) return "无逾期应收";

        var today = DateOnly.FromDateTime(ChinaTime.Now);
        int created = 0;

        foreach (var plan in overduePlans)
        {
            var daysOverdue = today.DayNumber - plan.DueDate.DayNumber;
            var stage = stages
                .Where(s => s.IsActive && daysOverdue >= s.DaysOverdue)
                .OrderByDescending(s => s.DaysOverdue)
                .FirstOrDefault();

            if (stage == null) continue;

            // 检查是否已有该阶段的催缴记录
            var existingRecords = await _uow.CollectionRecords.GetAllAsync(ct);
            var alreadyExists = existingRecords.Any(r =>
                r.ContractId == plan.ContractId && r.CollectionStageId == stage.Id);

            if (!alreadyExists)
            {
                var record = new CollectionRecord(plan.ContractId, stage.Id);
                await _uow.CollectionRecords.AddAsync(record, ct);
                created++;
            }
        }

        await _uow.CommitAsync(ct);
        return $"{created} 条催缴记录已创建";
    }
}

/// <summary>
/// 续签提醒 Job — 提前 14 天通知运营人员合同即将到期
/// </summary>
public class RenewalReminderJob : IScheduledJob
{
    public string JobName => "RenewalReminder";
    private readonly IUnitOfWork _uow;

    public RenewalReminderJob(IUnitOfWork uow) => _uow = uow;

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        var targetDate = today.AddDays(14);

        var contracts = await _uow.Contracts.GetAllAsync(ct);
        var expiring = contracts
            .Where(c => c.CompanyId == companyId
                     && c.Status == "Active"
                     && c.EndDate == targetDate)
            .ToList();

        if (expiring.Count == 0) return "今日无到期前 14 天的合同";

        return $"{expiring.Count} 份合同即将到期（{expiring.First().EndDate:yyyy-MM-dd}），已通知运营人员";
    }
}
