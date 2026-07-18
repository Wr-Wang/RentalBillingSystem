namespace RBS.Core.Entities.Scheduling;

using RBS.Core.Entities.Base;

/// <summary>
/// 任务执行日志 — 记录每次任务执行的完整信息（AuditableEntity）
/// 替换旧版的 ScheduledTaskLog，提供更丰富的执行状态跟踪。
/// 记录任务名称、所属公司、目标账期、触发方式、执行模式、
/// 执行时长、处理数量、摘要结果及错误信息等全链路数据
/// </summary>
public class TaskLog : AuditableEntity
{
    /// <summary>
    /// 任务名称，如 "租金计算"、"利息计算"，标识具体执行的任务
    /// </summary>
    public string TaskName { get; private set; } = string.Empty;

    /// <summary>
    /// 所属公司标识，用于多租户数据隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 关联合同标识（可选），当任务针对单个合同执行时填充
    /// </summary>
    public Guid? ContractId { get; private set; }

    /// <summary>
    /// 目标月份（账期），格式为 "yyyy-MM"，如 "2026-07"
    /// 标识任务处理的会计月份
    /// </summary>
    public string TargetMonth { get; private set; } = string.Empty;

    /// <summary>
    /// 触发类型：Scheduled=定时调度, Manual=手动触发, Auto=自动触发
    /// 默认值为 "Scheduled"
    /// </summary>
    public string TriggerType { get; private set; } = "Scheduled";

    /// <summary>
    /// 执行模式：Execute=正式执行, DryRun=试算/预览, Debug=调试模式
    /// 默认值为 "Execute"
    /// </summary>
    public string RunMode { get; private set; } = "Execute";

    /// <summary>
    /// 任务执行状态：Running=运行中, Completed=完成, Failed=失败, Stale=僵死
    /// 默认值为 "Running"
    /// </summary>
    public string Status { get; private set; } = "Running";

    /// <summary>
    /// 任务参数（可选），JSON 格式存储任务的执行参数快照，用于审计和追溯
    /// </summary>
    public string? Params { get; private set; }

    /// <summary>
    /// 任务开始执行时间（北京时间）
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// 任务完成时间（北京时间，可选），任务结束（成功/失败）时填充
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 任务总耗时（毫秒），由 CompletedAt - StartedAt 计算得出
    /// </summary>
    public int? TotalDurationMs { get; private set; }

    /// <summary>
    /// 总处理记录数，任务需要处理的总数据行数
    /// </summary>
    public int? TotalCount { get; private set; }

    /// <summary>
    /// 成功处理记录数，任务成功处理的数据行数
    /// </summary>
    public int? SuccessCount { get; private set; }

    /// <summary>
    /// 失败处理记录数，任务处理失败的数据行数
    /// </summary>
    public int? FailCount { get; private set; }

    /// <summary>
    /// 警告记录数，任务处理中产生警告的数据行数
    /// </summary>
    public int? WarningCount { get; private set; }

    /// <summary>
    /// 任务摘要（可选），任务执行结果的文字描述，如"成功处理 50 条，失败 2 条"
    /// </summary>
    public string? Summary { get; private set; }

    /// <summary>
    /// 最后心跳时间（可选），用于检测任务是否僵死。调度引擎定期更新此字段
    /// </summary>
    public DateTime? HeartbeatAt { get; private set; }

    /// <summary>
    /// 结果数据（可选），DryRun 模式下存储试算结果的 JSON 快照
    /// </summary>
    public string? ResultData { get; private set; }

    /// <summary>
    /// 错误信息（可选），任务失败时记录异常消息或堆栈信息
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private TaskLog() { }

    /// <summary>
    /// 创建任务执行日志实例。创建时自动记录开始时间（北京时间），状态设为 Running
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="targetMonth">目标月份，格式 "yyyy-MM"</param>
    /// <param name="triggerType">触发类型，默认 "Scheduled"</param>
    /// <param name="runMode">执行模式，默认 "Execute"</param>
    public TaskLog(string taskName, Guid companyId, string targetMonth,
        string triggerType = "Scheduled", string runMode = "Execute")
    {
        TaskName = taskName;
        CompanyId = companyId;
        TargetMonth = targetMonth;
        TriggerType = triggerType;
        RunMode = runMode;
        Status = "Running";
        StartedAt = RBS.Core.Common.ChinaTime.Now;
    }

    /// <summary>
    /// 标记任务执行完成。记录完成时间、处理计数、摘要及总耗时
    /// </summary>
    /// <param name="totalCount">总处理记录数</param>
    /// <param name="successCount">成功记录数</param>
    /// <param name="failCount">失败记录数</param>
    /// <param name="warningCount">警告记录数</param>
    /// <param name="summary">执行摘要</param>
    public void Complete(int totalCount, int successCount, int failCount, int warningCount, string? summary)
    {
        Status = "Completed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        TotalCount = totalCount;
        SuccessCount = successCount;
        FailCount = failCount;
        WarningCount = warningCount;
        Summary = summary;
        TotalDurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
    }

    /// <summary>
    /// 标记任务执行失败。记录错误信息和总耗时
    /// </summary>
    /// <param name="error">错误消息或异常信息</param>
    public void Fail(string error)
    {
        Status = "Failed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        ErrorMessage = error;
        TotalDurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
    }

    /// <summary>
    /// 将任务标记为僵死状态（Stale）。
    /// 当心跳超时（超过 10 分钟无心跳）由调度引擎调用，表示任务进程可能已丢失
    /// </summary>
    public void MarkStale()
    {
        Status = "Stale";
    }

    /// <summary>
    /// 更新任务心跳时间。由调度引擎定期调用，用于检测任务是否僵死
    /// </summary>
    public void UpdateHeartbeat()
    {
        HeartbeatAt = RBS.Core.Common.ChinaTime.Now;
    }

    /// <summary>
    /// 设置试算（DryRun）结果。将任务标记为完成并保存结果数据快照
    /// </summary>
    /// <param name="resultData">试算结果的 JSON 数据</param>
    public void SetDryRunResult(string resultData)
    {
        ResultData = resultData;
        Status = "Completed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
    }
}
