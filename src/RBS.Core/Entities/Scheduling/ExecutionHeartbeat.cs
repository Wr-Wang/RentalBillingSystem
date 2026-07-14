namespace RBS.Core.Entities.Scheduling;

/// <summary>
/// 排期心跳日志 — 独立于 TaskLog，仅用于进程健康探活
/// 调度引擎每 30 秒写入一条心跳记录，超过 10 分钟无心跳则判定为任务僵死（Stale）。
/// 使用自增 long 主键，高频写入时性能优于 GUID
/// </summary>
public class ExecutionHeartbeat
{
    /// <summary>
    /// 心跳记录自增标识（数据库自增 long），用于高频写入场景
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    /// 执行实例标识，关联到 JobScheduleExecution
    /// </summary>
    public Guid ExecutionId { get; private set; }

    /// <summary>
    /// 排期配置标识，关联到 JobSchedule
    /// </summary>
    public Guid JobScheduleId { get; private set; }

    /// <summary>
    /// 任务名称，冗余字段便于日志查询
    /// </summary>
    public string JobName { get; private set; } = string.Empty;

    /// <summary>
    /// 所属公司标识，用于多租户数据隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 目标月份（账期），格式 "yyyy-MM"，冗余字段便于日志查询
    /// </summary>
    public string TargetMonth { get; private set; } = string.Empty;

    /// <summary>
    /// 心跳时间（北京时间），记录本次探活的时间戳
    /// </summary>
    public DateTime HeartbeatAt { get; private set; }

    /// <summary>
    /// 记录创建时间（北京时间），与 HeartbeatAt 基本一致，用于数据库排序
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ExecutionHeartbeat() { }

    /// <summary>
    /// 创建心跳记录实例。自动记录心跳时间（北京时间）
    /// </summary>
    /// <param name="executionId">执行实例标识</param>
    /// <param name="jobScheduleId">排期配置标识</param>
    /// <param name="jobName">任务名称</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="targetMonth">目标月份，格式 "yyyy-MM"</param>
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
