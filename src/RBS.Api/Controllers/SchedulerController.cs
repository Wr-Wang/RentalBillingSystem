using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Application.Services.Scheduling;
using Dapper;
using RBS.Core.Common;
using RBS.Core.Entities.Scheduling;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerService _jobService;
    private readonly IJobTemplateService _templateService;
    private readonly IJobScheduleExecutionService _executionService;
    private readonly IEnumerable<IScheduledJob> _jobs;
    private readonly ITaskLogRepository _taskLogRepo;
    private readonly ITaskStepLogRepository _stepLogRepo;
    private readonly ISqlLoader _sql;
    private readonly IDbConnectionFactory _db;
    private readonly ITaskStepLogger _stepLogger;
    private readonly JobExecutionContext _jobContext;

    public SchedulerController(
        ISchedulerService jobService,
        IJobTemplateService templateService,
        IJobScheduleExecutionService executionService,
        IEnumerable<IScheduledJob> jobs,
        ITaskLogRepository taskLogRepo,
        ITaskStepLogRepository stepLogRepo,
        ISqlLoader sql,
        IDbConnectionFactory db,
        ITaskStepLogger stepLogger,
        JobExecutionContext jobContext)
    {
        _jobService = jobService;
        _templateService = templateService;
        _executionService = executionService;
        _jobs = jobs;
        _taskLogRepo = taskLogRepo;
        _stepLogRepo = stepLogRepo;
        _sql = sql;
        _db = db;
        _stepLogger = stepLogger;
        _jobContext = jobContext;
    }

    // ===== 模板 =====
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken ct)
        => Ok(await _templateService.GetAllAsync(ct));

    // ===== 任务定义 =====
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(CancellationToken ct)
        => Ok(await _jobService.GetJobsAsync(ct));

    [HttpPost("jobs")]
    public async Task<IActionResult> Create([FromBody] CreateJobScheduleRequest request, CancellationToken ct)
    {
        var result = await _jobService.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("jobs/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobScheduleRequest request, CancellationToken ct)
    {
        await _jobService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _jobService.DeleteAsync(id, ct);
        return NoContent();
    }

    // ===== 执行排期 =====
    [HttpGet("jobs/{jobId}/executions")]
    public async Task<IActionResult> GetExecutions(Guid jobId,
        [FromQuery] int months = 6, CancellationToken ct = default)
        => Ok(await _executionService.GetExecutionsAsync(jobId, months, ct));

    [HttpGet("jobs/{jobId}/executions/{id}")]
    public async Task<IActionResult> GetExecution(Guid jobId, Guid id, CancellationToken ct)
        => Ok(await _executionService.GetByIdAsync(id, ct));

    [HttpPost("jobs/{jobId}/executions")]
    public async Task<IActionResult> CreateExecution(Guid jobId,
        [FromBody] CreateExecutionRequest request, CancellationToken ct)
    {
        var result = await _executionService.CreateAsync(jobId, request, ct);
        return Ok(result);
    }

    [HttpPut("jobs/{jobId}/executions/{id}")]
    public async Task<IActionResult> UpdateExecution(Guid jobId, Guid id,
        [FromBody] UpdateExecutionRequest request, CancellationToken ct)
    {
        await _executionService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("jobs/{jobId}/executions/{id}")]
    public async Task<IActionResult> DeleteExecution(Guid jobId, Guid id, CancellationToken ct)
    {
        await _executionService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("jobs/{jobId}/executions/generate")]
    public async Task<IActionResult> GenerateExecutions(Guid jobId, CancellationToken ct)
    {
        var result = await _executionService.GenerateAsync(jobId, ct);
        return Ok(new { generated = result.Count, items = result });
    }

    /// <summary>重试失败排期：同步执行完整闭环（重置→抢占→执行→更新状态→记录日志）</summary>
    [HttpPost("jobs/{jobId}/executions/{id}/retry")]
    public async Task<IActionResult> RetryExecution(Guid jobId, Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        // 1. 加载排期
        var execution = await Dapper.SqlMapper.QuerySingleOrDefaultAsync<JobScheduleExecution>(conn,
            _sql.Get("Scheduling.Select.Execution.FailedById"),
            new { Id = id });
        if (execution == null)
            return NotFound(new { error = "排期不存在或状态不是 Failed" });

        // 2. 加载任务定义
        var schedule = await Dapper.SqlMapper.QuerySingleOrDefaultAsync<JobSchedule>(conn,
            _sql.Get("Scheduling.Select.JobSchedule.ActiveById"),
            new { Id = jobId });
        if (schedule == null)
            return NotFound(new { error = "任务不存在或已停用" });

        // 3. 查找作业实现
        var jobDict = _jobs.ToDictionary(j => j.JobName, StringComparer.OrdinalIgnoreCase);
        if (!jobDict.TryGetValue(schedule.JobName, out var job))
            return BadRequest(new { error = $"未找到作业实现: {schedule.JobName}" });

        // 4. 原子抢占
        var claimed = await Dapper.SqlMapper.ExecuteAsync(conn,
            _sql.Get("Scheduling.Update.Execution.Claim"),
            new { Id = execution.Id });
        if (claimed == 0)
            return Conflict(new { error = "排期已被其他进程抢占" });

        // 5. 创建任务日志（TriggerType=Retry）
        var taskLogId = Guid.NewGuid();
        await Dapper.SqlMapper.ExecuteAsync(conn,
            _sql.Get("Scheduling.Insert.TaskLog.Default"),
            new
            {
                Id = taskLogId,
                TaskName = schedule.JobName,
                CompanyId = execution.CompanyId,
                ContractId = (Guid?)null,
                TargetMonth = execution.Month,
                TriggerType = "Retry",
                RunMode = "Execute",
                Status = "Running",
                StartedAt = ChinaTime.Now,
                CreatedBy = Guid.Empty
            });
        _jobContext.TaskLogId = taskLogId;

        // 6. 步骤日志
        Guid? stepId = null;
        try { stepId = await _stepLogger.StartStepAsync(taskLogId, "Schedule.Execute", $"重试 {schedule.JobName}", null, null, ct); }
        catch { /* 步骤日志失败不影响 */ }

        try
        {
            // 7. 执行任务
            var result = await job.ExecuteAsync(execution.CompanyId, execution.Month, ct);

            if (stepId.HasValue)
                try { await _stepLogger.CompleteStepAsync(stepId.Value, 1, null, ct); } catch { }

            // 8. 更新排期状态
            await Dapper.SqlMapper.ExecuteAsync(conn,
                _sql.Get("Scheduling.Update.Execution.Complete"),
                new { Id = execution.Id });

            // 9. 更新任务日志（直接 Dapper 更新，与调度引擎风格一致）
            await Dapper.SqlMapper.ExecuteAsync(conn,
                _sql.Get("Scheduling.Update.TaskLog.CompleteByName"),
                new { Status = "Completed", Now = ChinaTime.Now, Name = schedule.JobName, Cid = execution.CompanyId, Month = execution.Month });

            return Ok(new { status = "Completed", message = result });
        }
        catch (Exception ex)
        {
            if (stepId.HasValue)
                try { await _stepLogger.FailStepAsync(stepId.Value, ex.Message, null, ct); } catch { }

            await Dapper.SqlMapper.ExecuteAsync(conn,
                _sql.Get("Scheduling.Update.Execution.Fail"),
                new { Id = execution.Id, Reason = ex.Message });

            await Dapper.SqlMapper.ExecuteAsync(conn,
                _sql.Get("Scheduling.Update.TaskLog.FailByName"),
                new { Status = "Failed", Now = ChinaTime.Now, Error = ex.Message, Name = schedule.JobName, Cid = execution.CompanyId, Month = execution.Month });

            return Ok(new { status = "Failed", error = ex.Message });
        }
    }

    /// <summary>跳过排期（Failed/Pending → Skipped）</summary>
    [HttpPost("jobs/{jobId}/executions/{id}/skip")]
    public async Task<IActionResult> SkipExecution(Guid jobId, Guid id, [FromBody] Dictionary<string, string>? body, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var affected = await Dapper.SqlMapper.ExecuteAsync(conn,
            _sql.Get("Scheduling.Update.Execution.Skip"),
            new { Id = id, Reason = body?.GetValueOrDefault("reason") ?? "手动跳过" });
        return affected > 0 ? Ok(new { status = "Skipped" }) : NotFound(new { error = "操作失败，状态不允许跳过" });
    }

    /// <summary>暂停排期（Pending → Paused）</summary>
    [HttpPost("jobs/{jobId}/executions/{id}/pause")]
    public async Task<IActionResult> PauseExecution(Guid jobId, Guid id, [FromBody] Dictionary<string, string>? body, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var affected = await Dapper.SqlMapper.ExecuteAsync(conn,
            _sql.Get("Scheduling.Update.Execution.Pause"),
            new { Id = id, Reason = body?.GetValueOrDefault("reason") ?? "手动暂停" });
        return affected > 0 ? Ok(new { status = "Paused" }) : NotFound(new { error = "操作失败，状态不允许暂停" });
    }

    /// <summary>取消排期（Pending/Skipped/Paused → Cancelled）</summary>
    [HttpPost("jobs/{jobId}/executions/{id}/cancel")]
    public async Task<IActionResult> CancelExecution(Guid jobId, Guid id, [FromBody] Dictionary<string, string>? body, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var affected = await Dapper.SqlMapper.ExecuteAsync(conn,
            _sql.Get("Scheduling.Update.Execution.Cancel"),
            new { Id = id, Reason = body?.GetValueOrDefault("reason") ?? "手动取消" });
        return affected > 0 ? Ok(new { status = "Cancelled" }) : NotFound(new { error = "操作失败，状态不允许取消" });
    }

    /// <summary>恢复排期（Paused/Skipped → Pending）</summary>
    [HttpPost("jobs/{jobId}/executions/{id}/resume")]
    public async Task<IActionResult> ResumeExecution(Guid jobId, Guid id, [FromBody] Dictionary<string, string>? body, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var affected = await Dapper.SqlMapper.ExecuteAsync(conn,
            _sql.Get("Scheduling.Update.Execution.Resume"),
            new { Id = id, Reason = body?.GetValueOrDefault("reason") ?? "手动恢复" });
        return affected > 0 ? Ok(new { status = "Pending" }) : NotFound(new { error = "操作失败，状态不允许恢复" });
    }

    // ===== 任务执行（★新增）=====

    /// <summary>执行任务（自动/手动/预执行）</summary>
    [HttpPost("execute/{jobName}")]
    public async Task<IActionResult> Execute(string jobName,
        [FromBody] JobExecuteRequest request, CancellationToken ct)
    {
        var job = _jobs.FirstOrDefault(j =>
            j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));
        if (job == null)
            return NotFound(new { error = $"任务 {jobName} 不存在" });

        var mode = request.Mode?.ToLower() switch
        {
            "dry-run" or "dryrun" or "预执行" => ExecuteMode.DryRun,
            _ => ExecuteMode.Execute
        };

        if (mode == ExecuteMode.DryRun)
        {
            if (job is ScheduledJobBase dryRunJob)
            {
                var report = await dryRunJob.DryRunAsync(request.CompanyId, request.TargetMonth, ct);
                return Ok(new { mode = "dry-run", report });
            }
            return Ok(new { mode = "dry-run", message = "该任务不支持 DryRun" });
        }

        if (job is ScheduledJobBase baseJob)
        {
            var execResult = await baseJob.ExecuteWithOptionsAsync(request, ct);
            return Ok(new { mode = "execute", result = execResult });
        }
        var jobResult = await job.ExecuteAsync(request.CompanyId, request.TargetMonth, ct);
        return Ok(new { mode = "execute", result = jobResult });
    }

    [HttpDelete("jobs/{jobId}/executions/future")]
    public async Task<IActionResult> DeleteFutureExecutions(Guid jobId, CancellationToken ct)
    {
        await _executionService.DeleteFutureAsync(jobId, ct);
        return Ok(new { message = "未来排期已删除" });
    }

    /// <summary>反转出账（取消本次任务产生的所有数据）</summary>
    [HttpPost("reverse/{taskLogId}")]
    public async Task<IActionResult> Reverse(Guid taskLogId, [FromBody] ReverseRequest body, CancellationToken ct)
    {
        var log = await _taskLogRepo.GetByIdAsync(taskLogId, ct);
        if (log == null) return NotFound(new { error = "任务日志不存在" });

        if (log.TaskName != "BillJob")
            return BadRequest(new { error = "仅支持 BillJob 反转" });

        using var conn = ((RBS.Core.Interfaces.Persistence.IDbConnectionFactory)
            HttpContext.RequestServices.GetRequiredService(typeof(RBS.Core.Interfaces.Persistence.IDbConnectionFactory))).CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        // 安全检查：该期是否有收款
        var hasPayment = await conn.QuerySingleAsync<int>(
            _sql.Get("Scheduling.Select.Receipt.HasPaymentByPeriod"), new { P = log.TargetMonth });
        if (hasPayment > 0)
            return BadRequest(new { error = $"该账期已有收款记录，禁止反转" });

        // 反转：删除本次出账产生的数据
        var taskStart = log.StartedAt;
        var taskEnd = log.CompletedAt ?? ChinaTime.Now;

        // 1. 标记账单为 Cancelled（按时间范围匹配）
        await conn.ExecuteAsync(
            _sql.Get("Scheduling.Update.DebitNote.CancelByTaskLog"),
            new { Start = taskStart, End = taskEnd, Reason = body?.Reason ?? "管理员反转" }, tx);

        // 2. 删除 Journal
        await conn.ExecuteAsync(
            _sql.Get("Scheduling.Delete.Journal.ByTimeRange"),
            new { Start = taskStart, End = taskEnd }, tx);

        // 3. 删除按账期和时间的 Journal
        await conn.ExecuteAsync(
            _sql.Get("Scheduling.Delete.Journal.ByPeriodTimeRange"),
            new { P = log.TargetMonth, Start = taskStart, End = taskEnd }, tx);

        // 4. 标记任务为 Reversed
        await conn.ExecuteAsync(
            _sql.Get("Scheduling.Update.TaskLog.Reversed"), new { Id = taskLogId }, tx);

        tx.Commit();
        return Ok(new { message = "反转成功，已删除该次出账数据" });
    }

    // ===== 任务日志（★新增）=====

    /// <summary>查询任务执行日志</summary>
    [HttpGet("tasklogs")]
    public async Task<IActionResult> GetTaskLogs(
        [FromQuery] string? taskName, [FromQuery] Guid? companyId,
        [FromQuery] string? targetMonth, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(taskName) || companyId == null)
            return Ok(new List<object>());

        var logs = await _taskLogRepo.GetByTaskNameAsync(
            taskName, companyId.Value, targetMonth ?? "", ct);
        return Ok(logs.Select(l => new
        {
            l.Id, l.TaskName, l.CompanyId, l.TargetMonth,
            l.TriggerType, l.RunMode, l.Status,
            l.StartedAt, l.CompletedAt, l.TotalDurationMs,
            l.TotalCount, l.SuccessCount, l.FailCount, l.WarningCount,
            l.Summary, l.ErrorMessage
        }));
    }

    /// <summary>获取任务日志详情</summary>
    [HttpGet("tasklogs/{id}")]
    public async Task<IActionResult> GetTaskLog(Guid id, CancellationToken ct)
    {
        var log = await _taskLogRepo.GetByIdAsync(id, ct);
        if (log == null) return NotFound();
        return Ok(new
        {
            log.Id, log.TaskName, log.CompanyId, log.TargetMonth,
            log.TriggerType, log.RunMode, log.Status,
            log.StartedAt, log.CompletedAt, log.TotalDurationMs,
            log.TotalCount, log.SuccessCount, log.FailCount, log.WarningCount,
            log.Summary, log.ErrorMessage, log.ResultData
        });
    }

    /// <summary>获取任务步骤日志</summary>
    [HttpGet("tasklogs/{id}/steps")]
    public async Task<IActionResult> GetTaskSteps(Guid id, CancellationToken ct)
    {
        var steps = await _stepLogRepo.GetByTaskLogIdAsync(id, ct);
        return Ok(steps.Select(s => new
        {
            s.Id, s.TaskLogId, s.StepName, s.StepDisplayName,
            s.ParentId, s.Status, s.StartedAt, s.CompletedAt,
            s.DurationMs, s.AffectedCount, s.Message, s.ErrorMessage
        }));
    }

    /// <summary>获取排期心跳日志</summary>
    [HttpGet("executions/{id}/heartbeats")]
    public async Task<IActionResult> GetExecutionHeartbeats(Guid id, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var beats = await Dapper.SqlMapper.QueryAsync(conn,
            _sql.Get("Scheduling.Select.ExecutionHeartbeat.ByExecutionId"),
            new { Id = id, Take = take });
        return Ok(beats);
    }

    /// <summary>手动标记任务日志为已完成（用于修复卡住的 Running/Processing 状态）</summary>
    [HttpPut("tasklogs/{id}/complete")]
    public async Task<IActionResult> CompleteTaskLog(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var affected = await Dapper.SqlMapper.ExecuteAsync(conn,
            "UPDATE TaskLogs SET Status='Completed', CompletedAt=DATEADD(HOUR, 8, GETUTCDATE()) WHERE Id=@Id AND Status IN ('Running','Processing')",
            new { Id = id });
        if (affected == 0)
            return NotFound(new { error = "未找到该日志或状态不允许修改" });
        return Ok(new { status = "Completed" });
    }

    // ===== 日记账历史数据清理 =====

    private bool IsSuperAdmin => User.FindFirst("IsSuperAdmin")?.Value == "True";
    private IActionResult? RequireSuperAdmin() => IsSuperAdmin ? null : Forbid();

    /// <summary>清理日记账历史数据（仅超级管理员）</summary>
    [HttpDelete("cleanup-journal-history")]
    public async Task<IActionResult> CleanupJournalHistory(CancellationToken ct = default)
    {
        var auth = RequireSuperAdmin();
        if (auth != null) return auth;

        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var result = new
            {
                DeletedOrphanGLEntries = await conn.ExecuteAsync(
                    _sql.Get("Accounting.Delete.GLEntry.OrphanJournalPost"), transaction: tx),
            };

            tx.Commit();

            // 记录清理日志（直接写 SystemLogs）
            try
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO SystemLogs (Id, Level, Category, Message, MachineName, CreatedAt)
                      VALUES (@Id, 'Info', 'DataCleanup', @Msg, @Machine, DATEADD(HOUR, 8, GETUTCDATE()))",
                    new
                    {
                        Id = Guid.NewGuid(),
                        Msg = $"数据清理完成: OrphanGLEntries={result.DeletedOrphanGLEntries}",
                        Machine = Environment.MachineName
                    });
            }
            catch { /* 日志记录失败不影响主流程 */ }

            return Ok(result);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return StatusCode(500, new { error = $"清理失败: {ex.Message}" });
        }
    }
}

public class ReverseRequest
{
    public string? Reason { get; set; }
}
