namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

/// <summary>
/// 定时任务执行日志 — 记录定时调度任务的每次执行状态（AuditableEntity）
/// 用于追踪定时任务的执行情况，包括开始/完成时间、执行状态、目标账期和心跳检测。
/// 注意：此实体为旧版调度日志，新版调度模块使用 Scheduling/TaskLog 替代
/// </summary>
public class ScheduledTaskLog : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 任务名称，标识具体的定时任务类型
    /// </summary>
    public string TaskName { get; private set; } = string.Empty;

    /// <summary>
    /// 任务开始执行时间（北京时间，可选）
    /// </summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>
    /// 任务完成时间（北京时间，可选）
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 任务执行状态：Pending=待执行, Running=运行中, Completed=已完成, Failed=失败, Stale=僵死
    /// 默认值为 "Pending"
    /// </summary>
    public string Status { get; private set; } = "Pending";

    /// <summary>
    /// 错误信息（可选），任务失败时记录异常消息
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// 所属公司标识，用于多租户数据隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 目标月份（账期），格式 "yyyy-MM"，是执行锁的核心字段。
    /// 用于防止同公司同月任务重复执行
    /// </summary>
    public string? TargetMonth { get; private set; }

    /// <summary>
    /// 心跳时间（北京时间，可选），用于检测僵死任务。
    /// 调度引擎定期更新，超过阈值无心跳则判定为 Stale
    /// </summary>
    public DateTime? HeartbeatAt { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ScheduledTaskLog() { }

    /// <summary>
    /// 创建定时任务日志实例。初始状态为 Pending
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <param name="companyId">所属公司标识</param>
    public ScheduledTaskLog(string taskName, Guid companyId) { TaskName = taskName; CompanyId = companyId; }

    /// <summary>
    /// 将任务标记为运行中，设置目标月份并记录开始时间和初始心跳
    /// </summary>
    /// <param name="targetMonth">目标月份，格式 "yyyy-MM"</param>
    public void SetRunning(string targetMonth)
    {
        Status = "Running";
        TargetMonth = targetMonth;
        StartedAt = ChinaTime.Now;
        HeartbeatAt = ChinaTime.Now;
    }

    /// <summary>更新任务心跳时间，由调度引擎定期调用</summary>
    public void Heartbeat() => HeartbeatAt = ChinaTime.Now;

    /// <summary>标记任务执行完成</summary>
    public void Complete()
    {
        Status = "Completed";
        CompletedAt = ChinaTime.Now;
    }

    /// <summary>标记任务执行失败，记录错误信息</summary>
    /// <param name="errorMessage">错误消息</param>
    public void Fail(string errorMessage)
    {
        Status = "Failed";
        ErrorMessage = errorMessage;
        CompletedAt = ChinaTime.Now;
    }

    /// <summary>
    /// 将运行中的任务标记为僵死状态（Stale）。
    /// 当心跳超时时由调度引擎调用，表示该任务进程可能已丢失
    /// </summary>
    public void MarkStale()
    {
        if (Status == "Running") Status = "Stale";
    }
}
