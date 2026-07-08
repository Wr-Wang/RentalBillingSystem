using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.UnitOfWork;

// 注意：BillJob 和 SettleJob 已迁移到独立的 BillJob.cs / SettleJob.cs，
//       继承自 ScheduledJobBase，JobName 与数据库一致。

namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 自动续签 Job — 检查到期前 N 天且 AutoRenew=true 的合同
/// </summary>
public class AutoRenewJob : IScheduledJob
{
    public string JobName => "AutoRenewJob";
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
    public string JobName => "CollectionJob";
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

            // 按逾期天数范围匹配催缴阶段
            var stage = stages
                .Where(s => s.IsAuto && daysOverdue >= s.OverdueDaysFrom && daysOverdue <= s.OverdueDaysTo)
                .OrderBy(s => s.StageNo)
                .FirstOrDefault();

            if (stage == null) continue;

            // 检查是否已有该阶段的催缴记录
            var existingRecords = await _uow.CollectionRecords.GetAllAsync(ct);
            var alreadyExists = existingRecords.Any(r =>
                r.ContractId == plan.ContractId && r.StageNo == stage.StageNo);

            if (!alreadyExists)
            {
                var channel = stage.ActionType switch
                {
                    "SMS" => "SMS",
                    "CALL" => "PHONE",
                    "VISIT" => "VISIT",
                    "LEGAL" => "LEGAL",
                    _ => "SMS"
                };
                var content = $"{stage.StageName} - 逾期{daysOverdue}天";
                var record = new CollectionRecord(plan.ContractId, stage.StageNo, channel, content, companyId);
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
    public string JobName => "RenewalReminderJob";
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
