using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Application.Services.Scheduling;
using Dapper;
using RBS.Core.Interfaces.Repositories;

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

    public SchedulerController(
        ISchedulerService jobService,
        IJobTemplateService templateService,
        IJobScheduleExecutionService executionService,
        IEnumerable<IScheduledJob> jobs,
        ITaskLogRepository taskLogRepo,
        ITaskStepLogRepository stepLogRepo)
    {
        _jobService = jobService;
        _templateService = templateService;
        _executionService = executionService;
        _jobs = jobs;
        _taskLogRepo = taskLogRepo;
        _stepLogRepo = stepLogRepo;
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
        var execResult2 = await job.ExecuteAsync(request.CompanyId, request.TargetMonth, ct);
        return Ok(new { mode = "execute", result = execResult2 });
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
            "SELECT COUNT(1) FROM JournalEntries je " +
            "JOIN AccountingSubjects s ON s.Id = je.AccountingSubjectId " +
            "WHERE s.Code = '1001' AND je.VoucherId IN " +
            "(SELECT Id FROM Vouchers WHERE SourceEntityId IN " +
            "(SELECT ContractId FROM ReceivablePlans WHERE Period = @P))", new { P = log.TargetMonth });
        if (hasPayment > 0)
            return BadRequest(new { error = $"该账期已有收款记录，禁止反转" });

        // 反转：删除本次出账产生的数据
        var taskStart = log.StartedAt;
        var taskEnd = log.CompletedAt ?? DateTime.UtcNow;

        // 1. 标记账单为 Cancelled
        await conn.ExecuteAsync(
            "UPDATE DebitNotes SET Status='Cancelled', CancelledAt=GETUTCDATE(), CancelReason=@Reason WHERE BillJobTaskLogId=@Id AND Status='Published'",
            new { Id = taskLogId, Reason = body?.Reason ?? "管理员反转" }, tx);

        // 2. 删除分录
        await conn.ExecuteAsync(
            "DELETE je FROM JournalEntries je " +
            "JOIN Vouchers v ON v.Id = je.VoucherId " +
            "WHERE v.SourceEntityType='ReceivablePlan' AND v.CreatedAt >= @Start AND v.CreatedAt <= @End",
            new { Start = taskStart, End = taskEnd }, tx);
        await conn.ExecuteAsync(
            "DELETE FROM Vouchers WHERE SourceEntityType='ReceivablePlan' AND CreatedAt >= @Start AND CreatedAt <= @End",
            new { Start = taskStart, End = taskEnd }, tx);

        // 3. 删除应收计划
        await conn.ExecuteAsync(
            "DELETE FROM ReceivablePlans WHERE Period=@P AND CreatedAt >= @Start AND CreatedAt <= @End",
            new { P = log.TargetMonth, Start = taskStart, End = taskEnd }, tx);

        // 4. 标记任务为 Reversed
        await conn.ExecuteAsync(
            "UPDATE TaskLogs SET Status='Reversed' WHERE Id=@Id", new { Id = taskLogId }, tx);

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
}

public class ReverseRequest
{
    public string? Reason { get; set; }
}
