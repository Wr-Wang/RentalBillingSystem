namespace RBS.Core.Entities.Scheduling;

/// <summary>
/// 排期心跳日志 — 独立于 TaskLog，仅用于进程健康探活
/// 每 30 秒写入一条，超过 10 分钟无心跳则判定为僵死
/// </summary>
public class ExecutionHeartbeat
{
    public long Id { get; private set; }
    public Guid ExecutionId { get; private set; }
    public Guid JobScheduleId { get; private set; }
    public string JobName { get; private set; } = string.Empty;
    public Guid CompanyId { get; private set; }
    public string TargetMonth { get; private set; } = string.Empty;
    public DateTime HeartbeatAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExecutionHeartbeat() { }

    public ExecutionHeartbeat(Guid executionId, Guid jobScheduleId, string jobName, Guid companyId, string targetMonth)
    {
        ExecutionId = executionId;
        JobScheduleId = jobScheduleId;
        JobName = jobName;
        CompanyId = companyId;
        TargetMonth = targetMonth;
        HeartbeatAt = RBS.Core.Common.ChinaTime.Now;
    }
}
