using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Scheduling;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Scheduling;

public abstract class ScheduledJobBase : IScheduledJob
{
    private static readonly SemaphoreSlim _companySemaphore = new(2, 2);
    protected const int ContractParallelism = 20;

    protected readonly ITaskLogRepository _taskLogRepo;
    protected readonly ITaskStepLogger _stepLogger;
    protected readonly IUnitOfWork _uow;
    protected readonly JobExecutionContext _jobContext;

    public abstract string JobName { get; }

    protected ScheduledJobBase(
        ITaskLogRepository taskLogRepo,
        ITaskStepLogger stepLogger,
        IUnitOfWork uow,
        JobExecutionContext jobContext)
    {
        _taskLogRepo = taskLogRepo;
        _stepLogger = stepLogger;
        _uow = uow;
        _jobContext = jobContext;
    }

    public async Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct = default)
    {
        await _companySemaphore.WaitAsync(ct);
        try
        {
            var result = await ExecuteCoreAsync(companyId, targetMonth, ExecuteMode.Execute, ct);
            return result.Summary;
        }
        finally
        {
            _companySemaphore.Release();
        }
    }

    /// <summary>带选项的执行入口（供 API 调用）</summary>
    public async Task<string> ExecuteWithOptionsAsync(JobExecuteRequest request, CancellationToken ct = default)
    {
        // 支持 force 跳过锁
        if (request.Force)
        {
            var result = await ExecuteCoreAsync(request.CompanyId, request.TargetMonth, ExecuteMode.Execute, ct);
            return result.Summary;
        }
        return await ExecuteAsync(request.CompanyId, request.TargetMonth, ct);
    }

    /// <summary>公开的 DryRun 入口（供 API 调用）</summary>
    public async Task<string> DryRunAsync(Guid companyId, string targetMonth, CancellationToken ct = default)
    {
        var taskLogId = await BeginTaskLogAsync(JobName, companyId, targetMonth,
            "Manual", "DryRun", null, ct);
        var report = await BuildDryRunReportAsync(companyId, targetMonth, taskLogId, ct);
        await SetDryRunResultAsync(taskLogId, report, ct);
        return report;
    }

    protected abstract Task<JobResult> ExecuteCoreAsync(
        Guid companyId, string targetMonth, ExecuteMode mode, CancellationToken ct);

    protected async Task<Guid> BeginTaskLogAsync(string taskName, Guid companyId,
        string targetMonth, string triggerType, string runMode, string? paramsJson, CancellationToken ct)
    {
        // 宿主已创建 TaskLog 时复用（调度引擎执行），避免唯一索引冲突
        if (_jobContext.TaskLogId != Guid.Empty)
            return _jobContext.TaskLogId;

        var taskLog = new TaskLog(taskName, companyId, targetMonth, triggerType, runMode);
        if (paramsJson != null)
        {
            var field = typeof(TaskLog).GetField("<Params>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(taskLog, paramsJson);
        }
        await _taskLogRepo.CreateAsync(taskLog, ct);
        _ = HeartbeatLoopAsync(taskLog.Id, ct);
        return taskLog.Id;
    }

    private async Task HeartbeatLoopAsync(Guid taskLogId, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await _taskLogRepo.UpdateHeartbeatAsync(taskLogId, ct); await Task.Delay(30000, ct); }
            catch { break; }
        }
    }

    protected virtual Task<string> BuildDryRunReportAsync(
        Guid companyId, string targetMonth, Guid taskLogId, CancellationToken ct)
    {
        return Task.FromResult("{\"message\":\"dry-run not implemented\"}");
    }

    protected async Task CompleteTaskLogAsync(Guid taskLogId, JobResult result, CancellationToken ct)
    {
        await _taskLogRepo.CompleteAsync(taskLogId,
            result.TotalCount, result.SuccessCount, result.FailCount,
            result.WarningCount, result.Summary, ct);
    }

    protected async Task FailTaskLogAsync(Guid taskLogId, string error, CancellationToken ct)
    {
        await _taskLogRepo.FailAsync(taskLogId, error, ct);
    }

    protected async Task SetDryRunResultAsync(Guid taskLogId, string resultData, CancellationToken ct)
    {
        await _taskLogRepo.SetDryRunResultAsync(taskLogId, resultData, ct);
    }
}

public enum ExecuteMode { Execute, DryRun }

public class JobResult
{
    public int TotalCount { get; }
    public int SuccessCount { get; }
    public int FailCount { get; }
    public int WarningCount { get; }
    public string Summary { get; }
    public List<(Guid ContractId, string Error)> Errors { get; }

    public JobResult(int success, int fail, IEnumerable<(Guid, string)>? errors = null,
        int warning = 0, string? summary = null)
    {
        TotalCount = success + fail;
        SuccessCount = success;
        FailCount = fail;
        WarningCount = warning;
        Errors = errors?.ToList() ?? new();
        Summary = summary ?? $"{SuccessCount}/{TotalCount} 完成";
    }
}

public class JobExecuteRequest
{
    public string Mode { get; set; } = "execute";
    public Guid CompanyId { get; set; }
    public string TargetMonth { get; set; } = "";
    public List<Guid>? ContractIds { get; set; }
    public bool Force { get; set; }
}
