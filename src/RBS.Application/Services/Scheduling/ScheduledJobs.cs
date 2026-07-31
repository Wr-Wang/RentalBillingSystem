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
    private readonly INotificationService _notificationService;

    public AutoRenewJob(IUnitOfWork uow, IRenewalService renewalService,
        IDbConnectionFactory db, ISqlLoader sql,
        ITaskStepLogger stepLogger, JobExecutionContext jobContext,
        INotificationService notificationService)
    {
        _uow = uow;
        _renewalService = renewalService;
        _db = db;
        _sql = sql;
        _stepLogger = stepLogger;
        _jobContext = jobContext;
        _notificationService = notificationService;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var taskLogId = _jobContext.TaskLogId;
        var today = ChinaTime.Now.Date;
        var targetDate = today.AddDays(7);

        // Step01: 查询到期合同
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

        // Step02: 逐合同提交续签审批
        var stepSubmit = await _stepLogger.StartStepAsync(taskLogId,
            "AutoRenew.Submit", "逐合同提交续签审批", null, null, ct);

        int submitted = 0;
        int failed = 0;
        var errors = new System.Collections.Concurrent.ConcurrentBag<(string ContractNo, string Error)>();

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
                        NewEndDate = contract.EndDate!.Value.AddMonths(12).ToString("yyyy-MM-dd"),
                        DepositHandling = "TRANSFER"
                    }, SystemUsers.Scheduler, ct);
                if (result != null) submitted++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add((contract.ContractNo ?? "未知", ex.Message));
            }
        }

        await _stepLogger.CompleteStepAsync(stepSubmit, submitted, null, ct);

        // Step03: 通知运营人员续签结果
        bool notified = false;
        var stepNotify = await _stepLogger.StartStepAsync(taskLogId,
            "AutoRenew.Notify", "推送自动续签结果通知", null, null, ct);
        try
        {
            var failDetail = failed > 0
                ? $"，{failed} 份失败（{string.Join("; ", errors.Take(3).Select(e => $"{e.ContractNo}:{e.Error}"))}）"
                : "";
            await _notificationService.NotifyRoleAsync("OpsSupervisor",
                $"自动续签任务完成",
                $"共 {submitted}/{expiring.Count} 份合同续签已提交{failDetail}",
                "Renewal", null, ct);
            notified = true;
            await _stepLogger.CompleteStepAsync(stepNotify, expiring.Count, null, ct);
        }
        catch
        {
            await _stepLogger.FailStepAsync(stepNotify, "通知发送失败", null, ct);
        }

        var summary = $"已为 {submitted}/{expiring.Count} 份合同提交自动续签";
        if (failed > 0) summary += $"（{failed} 份失败）";
        if (notified) summary += "，已通知运营人员";
        return summary;
    }
}

/// <summary>
/// 催缴 Job — 按逾期阶段触发催缴记录
/// </summary>
public class CollectionJob : IScheduledJob
{
    public string JobName => "CollectionJob";
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ITaskStepLogger _stepLogger;
    private readonly JobExecutionContext _jobContext;
    private readonly INotificationService _notificationService;
    private readonly IContractService _contractService;

    public CollectionJob(IUnitOfWork uow,
        IDbConnectionFactory db, ISqlLoader sql,
        ITaskStepLogger stepLogger, JobExecutionContext jobContext,
        INotificationService notificationService,
        IContractService contractService)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
        _stepLogger = stepLogger;
        _jobContext = jobContext;
        _notificationService = notificationService;
        _contractService = contractService;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var taskLogId = _jobContext.TaskLogId;

        // Step: 加载配置
        var stepLoad = await _stepLogger.StartStepAsync(taskLogId,
            "Collection.Load", "加载逾期应收和催缴阶段配置", null, null, ct);

        var stages = await _uow.CollectionStages.GetAllAsync(ct);
        using var conn = _db.CreateConnection(); conn.Open();
        var overdueJournals = (await conn.QueryAsync(
            _sql.Get("Billing.Select.Journal.OverdueByCompany"),
            new { CompanyId = companyId })).ToList();

        await _stepLogger.CompleteStepAsync(stepLoad, overdueJournals.Count, null, ct);

        if (overdueJournals.Count == 0)
            return "无逾期应收";

        // Step: 创建催缴
        var stepCreate = await _stepLogger.StartStepAsync(taskLogId,
            "Collection.Create", "按逾期阶段创建催缴记录", null, null, ct);

        var today = ChinaTime.Now.Date;
        int created = 0;

        // 预加载已存在的催缴记录（移至循环外，避免重复查询全表）
        var existingRecords = await _uow.CollectionRecords.GetAllAsync(ct);
        var dedupSet = new HashSet<(Guid, int)>(existingRecords.Select(r =>
            (r.ContractId, r.StageNo)));

        // 跟踪本次创建的催缴记录信息（用于后续通知）
        var createdRecords = new List<(Guid ContractId, Guid RecordId, int StageNo, int DaysOverdue)>();

        foreach (var journal in overdueJournals)
        {
            var daysOverdue = (int)today.Subtract((DateTime)journal.DueDate).TotalDays;

            var stage = stages
                .Where(s => s.IsAuto && daysOverdue >= s.OverdueDaysFrom && daysOverdue <= s.OverdueDaysTo)
                .OrderBy(s => s.StageNo)
                .FirstOrDefault();

            if (stage == null) continue;

            var key = ((Guid)journal.ContractId, stage.StageNo);
            if (!dedupSet.Add(key)) continue; // HashSet 去重，O(1)

            var channel = stage.ActionType switch
            {
                "SMS" => "SMS",
                "CALL" => "PHONE",
                "VISIT" => "VISIT",
                "LEGAL" => "LEGAL",
                _ => "SMS"
            };
            var content = $"{stage.StageName} - 逾期{daysOverdue}天";
            var record = new CollectionRecord((Guid)journal.ContractId, stage.StageNo, channel, content, companyId);
            await _uow.CollectionRecords.AddAsync(record, ct);
            createdRecords.Add(((Guid)journal.ContractId, record.Id, stage.StageNo, daysOverdue));
            created++;
        }

        await _uow.CommitAsync(ct);

        await _stepLogger.CompleteStepAsync(stepCreate, created, null, ct);

        // Step: 逐合同推送系统通知
        if (createdRecords.Count > 0)
        {
            var stepNotify = await _stepLogger.StartStepAsync(taskLogId,
                "Collection.Notify", "推送催缴系统通知", null, null, ct);
            try
            {
                // 批量获取合同号
                var contractIds = createdRecords.Select(r => r.ContractId).Distinct().ToList();
                var contractNoMap = await _contractService.GetIdNoPairsAsync(contractIds, ct);

                // 计算各合同欠费总额（从 overdueJournals 汇总）
                var overdueAmounts = overdueJournals
                    .GroupBy(j => (Guid)j.ContractId)
                    .ToDictionary(g => g.Key, g => g.Sum(j => (decimal)j.Amount - (decimal)(j.Received ?? 0)));

                // 按合同聚合阶段（一个合同可能进入多个阶段）
                var perContract = createdRecords
                    .GroupBy(r => r.ContractId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                int notifCount = 0;
                foreach (var kv in perContract)
                {
                    var cId = kv.Key;
                    var records = kv.Value;
                    var contractNo = contractNoMap.GetValueOrDefault(cId, "");
                    var totalOverdue = overdueAmounts.GetValueOrDefault(cId, 0);
                    var maxDays = records.Max(r => r.DaysOverdue);
                    var stages_ = records.Select(r => stages.FirstOrDefault(s => s.StageNo == r.StageNo)?.StageName ?? $"S{r.StageNo}");

                    var title = $"催缴通知 - {contractNo}";
                    var content = $"合同 {contractNo} 逾期 {maxDays} 天，欠费 ¥{totalOverdue:N2}，已进入 {string.Join("、", stages_)} 阶段";

                    // 用第一个催缴记录 ID 作为关联参考
                    await _notificationService.NotifyRoleAsync("OpsSupervisor", "System",
                        title, content, "CollectionRecord", records[0].RecordId,
                        companyId, ct);
                    notifCount++;
                }

                await _stepLogger.CompleteStepAsync(stepNotify, notifCount, null, ct);
            }
            catch (Exception ex)
            {
                await _stepLogger.FailStepAsync(stepNotify, $"通知发送失败: {ex.Message}", null, ct);
            }
        }

        return $"创建 {created} 条催缴记录，已推送系统通知";
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
    private readonly INotificationService _notificationService;
    private readonly IAutoRenewConfigService _autoRenewConfigService;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public RenewalReminderJob(IUnitOfWork uow,
        ITaskStepLogger stepLogger, JobExecutionContext jobContext,
        INotificationService notificationService,
        IAutoRenewConfigService autoRenewConfigService,
        IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _stepLogger = stepLogger;
        _jobContext = jobContext;
        _notificationService = notificationService;
        _autoRenewConfigService = autoRenewConfigService;
        _db = db;
        _sql = sql;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct)
    {
        var taskLogId = _jobContext.TaskLogId;

        // Step01: 加载自动续签配置
        var stepConfig = await _stepLogger.StartStepAsync(taskLogId,
            "Reminder.LoadConfig", "加载自动续签配置", null, null, ct);

        var config = await _autoRenewConfigService.GetByCompanyAsync(companyId, ct);
        var advanceDays = config?.AdvanceDays ?? 14;
        await _stepLogger.CompleteStepAsync(stepConfig, advanceDays, null, ct);

        // Step02: 查询到期合同
        var stepQuery = await _stepLogger.StartStepAsync(taskLogId,
            "Reminder.Query", $"查询到期前{advanceDays}天的合同", null, null, ct);

        var today = ChinaTime.Now.Date;
        var targetDate = today.AddDays(advanceDays);

        using var conn = _db.CreateConnection(); conn.Open();
        var expiring = (await conn.QueryAsync<dynamic>(
            _sql.Get("Lease.Select.Contract.Expiring"),
            new { Date = targetDate, CompanyId = companyId })).ToList();

        await _stepLogger.CompleteStepAsync(stepQuery, expiring.Count, null, ct);

        if (expiring.Count == 0)
            return $"今日无到期前 {advanceDays} 天的合同";

        // Step03: 通知运营人员
        bool notified = false;
        var stepNotify = await _stepLogger.StartStepAsync(taskLogId,
            "Reminder.Notify", "发送到期提醒通知", null, null, ct);
        try
        {
            var sampleDate = expiring.First().EndDate is DateTime ed
                ? ed.ToString("yyyy-MM-dd") : targetDate.ToString("yyyy-MM-dd");
            await _notificationService.NotifyRoleAsync("OpsSupervisor",
                $"合同到期提醒",
                $"{expiring.Count} 份合同即将于 {sampleDate} 到期（提前{advanceDays}天提醒），请及时处理续签",
                "Renewal", null, ct);
            notified = true;
            await _stepLogger.CompleteStepAsync(stepNotify, expiring.Count, null, ct);
        }
        catch
        {
            await _stepLogger.FailStepAsync(stepNotify, "通知发送失败", null, ct);
        }

        var result = $"{expiring.Count} 份合同即将到期（提前{advanceDays}天）";
        if (notified) result += "，已通知运营人员";
        return result;
    }
}
