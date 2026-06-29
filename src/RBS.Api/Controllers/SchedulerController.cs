using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerService _jobService;
    private readonly IJobTemplateService _templateService;
    private readonly IJobScheduleExecutionService _executionService;

    public SchedulerController(
        ISchedulerService jobService,
        IJobTemplateService templateService,
        IJobScheduleExecutionService executionService)
    {
        _jobService = jobService;
        _templateService = templateService;
        _executionService = executionService;
    }

    // ===== 模板 =====

    /// <summary>获取任务模板列表</summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken ct)
        => Ok(await _templateService.GetAllAsync(ct));

    // ===== 任务定义 =====

    /// <summary>获取公司任务列表</summary>
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(CancellationToken ct)
        => Ok(await _jobService.GetJobsAsync(ct));

    /// <summary>创建任务（可选从模板）</summary>
    [HttpPost("jobs")]
    public async Task<IActionResult> Create([FromBody] CreateJobScheduleRequest request, CancellationToken ct)
    {
        var result = await _jobService.CreateAsync(request, ct);
        return Ok(result);
    }

    /// <summary>编辑任务</summary>
    [HttpPut("jobs/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobScheduleRequest request, CancellationToken ct)
    {
        await _jobService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>删除任务（级联删除排期）</summary>
    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _jobService.DeleteAsync(id, ct);
        return NoContent();
    }

    // ===== 执行排期 =====

    /// <summary>获取排期列表（空则自动生成）</summary>
    [HttpGet("jobs/{jobId}/executions")]
    public async Task<IActionResult> GetExecutions(Guid jobId,
        [FromQuery] int months = 6, CancellationToken ct = default)
        => Ok(await _executionService.GetExecutionsAsync(jobId, months, ct));

    /// <summary>获取单条排期</summary>
    [HttpGet("jobs/{jobId}/executions/{id}")]
    public async Task<IActionResult> GetExecution(Guid jobId, Guid id, CancellationToken ct)
        => Ok(await _executionService.GetByIdAsync(id, ct));

    /// <summary>添加自定义排期</summary>
    [HttpPost("jobs/{jobId}/executions")]
    public async Task<IActionResult> CreateExecution(Guid jobId,
        [FromBody] CreateExecutionRequest request, CancellationToken ct)
    {
        var result = await _executionService.CreateAsync(jobId, request, ct);
        return Ok(result);
    }

    /// <summary>编辑排期</summary>
    [HttpPut("jobs/{jobId}/executions/{id}")]
    public async Task<IActionResult> UpdateExecution(Guid jobId, Guid id,
        [FromBody] UpdateExecutionRequest request, CancellationToken ct)
    {
        await _executionService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>删除排期</summary>
    [HttpDelete("jobs/{jobId}/executions/{id}")]
    public async Task<IActionResult> DeleteExecution(Guid jobId, Guid id, CancellationToken ct)
    {
        await _executionService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>生成默认排期（从 Cron 解析）</summary>
    [HttpPost("jobs/{jobId}/executions/generate")]
    public async Task<IActionResult> GenerateExecutions(Guid jobId, CancellationToken ct)
    {
        var result = await _executionService.GenerateAsync(jobId, ct);
        return Ok(new { generated = result.Count, items = result });
    }
}
