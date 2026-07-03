namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 定时作业接口 — 每个 Job 一个实现，由 SchedulingHostedService 按计划触发
/// </summary>
public interface IScheduledJob
{
    /// <summary>作业唯一标识（与 JobSchedule.JobName 匹配）</summary>
    string JobName { get; }

    /// <summary>执行作业逻辑</summary>
    /// <param name="companyId">所属公司 ID</param>
    /// <param name="targetMonth">目标账期 yyyy-MM</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>执行结果描述</returns>
    Task<string> ExecuteAsync(Guid companyId, string targetMonth, CancellationToken ct = default);
}
