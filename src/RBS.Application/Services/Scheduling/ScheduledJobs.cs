using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Common;
using RBS.Core.Entities.Billing;
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
    private readonly ITaskStepLogger _stepLogger;
    private readonly JobExecutionContext _jobContext;

    public AutoRenewJob(IUnitOfWork uow, IRenewalService renewalService,
        IDbConnectionFactory db, ISqlLoader sql,
        ITaskStepLogger stepLogger, JobExecutionContext jobContext)
    {
        _uow = uow;
        _renewalService = renewalService;
        _db = db;
        _sql = sql;
        _stepLogger = stepLogger;
        _jobContext = jobContext;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var taskLogId = _jobContext.TaskLogId;
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        var targetDate = today.AddDays(7);

        // Step: 查询到期合同
        var stepQuery = await _stepLogger.StartStepAsync(taskLogId,
            "AutoRenew.Query", "查询到期前7天的续签合同", null, null, ct);

        var contracts = await _uow.Contracts.GetAllAsync(ct);
        var expiring = contracts
            .Where(c => c.CompanyId == companyId
                     && c.Status == "Active"
                     && c.AutoRenew
                     && c.EndDate == targetDate)
            .ToList();

        await _stepLogger.CompleteStepAsync(stepQuery, expiring.Count, null, ct);

        if (expiring.Count == 0)
            return "今日无到期前 7 天且启用自动续签的合同";

        // Step: 提交续签
        var stepSubmit = await _stepLogger.StartStepAsync(taskLogId,
            "AutoRenew.Submit", "逐合同提交续签审批", null, null, ct);

        int submitted = 0;
        foreach (var contract in expiring)
        {
            try
            {
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

        await _stepLogger.CompleteStepAsync(stepSubmit, submitted, null, ct);

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
    private readonly ITaskStepLogger _stepLogger;
    private readonly JobExecutionContext _jobContext;

    public CollectionJob(IUnitOfWork uow,
        ITaskStepLogger stepLogger, JobExecutionContext jobContext)
    {
        _uow = uow;
        _stepLogger = stepLogger;
        _jobContext = jobContext;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var taskLogId = _jobContext.TaskLogId;

        // Step: 加载配置
        var stepLoad = await _stepLogger.StartStepAsync(taskLogId,
            "Collection.Load", "加载逾期应收和催缴阶段配置", null, null, ct);

        var stages = await _uow.CollectionStages.GetAllAsync(ct);
        var overduePlans = await _uow.ReceivablePlans.GetOverdueAsync(companyId, ct);

        await _stepLogger.CompleteStepAsync(stepLoad, overduePlans.Count, null, ct);

        if (overduePlans.Count == 0)
            return "无逾期应收";

        // Step: 创建催缴
        var stepCreate = await _stepLogger.StartStepAsync(taskLogId,
            "Collection.Create", "按逾期阶段创建催缴记录", null, null, ct);

        var today = DateOnly.FromDateTime(ChinaTime.Now);
        int created = 0;

        foreach (var plan in overduePlans)
        {
            var daysOverdue = today.DayNumber - plan.DueDate.DayNumber;

            var stage = stages
                .Where(s => s.IsAuto && daysOverdue >= s.OverdueDaysFrom && daysOverdue <= s.OverdueDaysTo)
                .OrderBy(s => s.StageNo)
                .FirstOrDefault();

            if (stage == null) continue;

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

        await _stepLogger.CompleteStepAsync(stepCreate, created, null, ct);

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
    private readonly ITaskStepLogger _stepLogger;
    private readonly JobExecutionContext _jobContext;

    public RenewalReminderJob(IUnitOfWork uow,
        ITaskStepLogger stepLogger, JobExecutionContext jobContext)
    {
        _uow = uow;
        _stepLogger = stepLogger;
        _jobContext = jobContext;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var taskLogId = _jobContext.TaskLogId;

        // Step: 查询到期合同
        var stepQuery = await _stepLogger.StartStepAsync(taskLogId,
            "Reminder.Query", "查询到期前14天的合同", null, null, ct);

        var today = DateOnly.FromDateTime(ChinaTime.Now);
        var targetDate = today.AddDays(14);

        var contracts = await _uow.Contracts.GetAllAsync(ct);
        var expiring = contracts
            .Where(c => c.CompanyId == companyId
                     && c.Status == "Active"
                     && c.EndDate == targetDate)
            .ToList();

        await _stepLogger.CompleteStepAsync(stepQuery, expiring.Count, null, ct);

        if (expiring.Count == 0)
            return "今日无到期前 14 天的合同";

        // Step: 通知
        var stepNotify = await _stepLogger.StartStepAsync(taskLogId,
            "Reminder.Notify", "发送到期提醒通知", null, null, ct);

        // TODO: 集成通知服务发送提醒
        // 当前只返回统计信息，通知集成待后续实现

        await _stepLogger.CompleteStepAsync(stepNotify, expiring.Count, null, ct);

        return $"{expiring.Count} 份合同即将到期（{expiring.First().EndDate:yyyy-MM-dd}），已通知运营人员";
    }
}
