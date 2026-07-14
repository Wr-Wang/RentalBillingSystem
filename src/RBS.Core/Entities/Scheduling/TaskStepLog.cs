namespace RBS.Core.Entities.Scheduling;

/// <summary>
/// 任务步骤执行日志 — 记录任务中每个步骤的耗时和影响数
/// 步骤日志与业务数据在同一事务中写入，提交=记录，回滚=不留痕。
/// 支持多级子步骤（ParentId），适用于复杂任务的嵌套步骤监控
/// </summary>
public class TaskStepLog
{
    /// <summary>
    /// 步骤日志唯一标识
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 所属任务执行日志标识，关联到 TaskLog
    /// </summary>
    public Guid TaskLogId { get; private set; }

    /// <summary>
    /// 步骤名称（内部编码），如 "CalculateRent"、"GenerateBill"，用于程序识别
    /// </summary>
    public string StepName { get; private set; } = string.Empty;

    /// <summary>
    /// 步骤显示名称，如 "计算租金"、"生成账单"，用于界面展示
    /// </summary>
    public string StepDisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// 父步骤标识（可选），支持子步骤嵌套。为 null 表示顶层步骤
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// 排序序号，同一任务/父步骤下的步骤按此值升序执行
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 步骤执行状态：Running=运行中, Completed=已完成, Failed=失败, Skipped=跳过
    /// 默认值为 "Running"
    /// </summary>
    public string Status { get; private set; } = "Running";

    /// <summary>
    /// 步骤开始执行时间（北京时间）
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// 步骤完成时间（北京时间，可选），步骤结束（成功/失败/跳过）时填充
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 步骤耗时（毫秒），由 CompletedAt - StartedAt 计算得出
    /// </summary>
    public int? DurationMs { get; private set; }

    /// <summary>
    /// 影响记录数，该步骤处理/生成的数据行数
    /// </summary>
    public int? AffectedCount { get; private set; }

    /// <summary>
    /// 步骤消息（可选），记录步骤的补充说明，如跳过的原因
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// 错误信息（可选），步骤失败时记录异常消息
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private TaskStepLog() { }

    /// <summary>
    /// 创建步骤执行日志实例。自动生成 Id 并记录开始时间（北京时间），状态设为 Running
    /// </summary>
    /// <param name="taskLogId">所属任务日志标识</param>
    /// <param name="stepName">步骤内部编码名称</param>
    /// <param name="displayName">步骤显示名称</param>
    /// <param name="parentId">父步骤标识（可选），用于嵌套子步骤</param>
    /// <param name="sortOrder">排序序号，默认 0</param>
    public TaskStepLog(Guid taskLogId, string stepName, string displayName, Guid? parentId = null, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        TaskLogId = taskLogId;
        StepName = stepName;
        StepDisplayName = displayName;
        ParentId = parentId;
        SortOrder = sortOrder;
        Status = "Running";
        StartedAt = RBS.Core.Common.ChinaTime.Now;
    }

    /// <summary>
    /// 标记步骤执行完成。记录影响记录数及耗时
    /// </summary>
    /// <param name="affectedCount">该步骤影响的记录数</param>
    public void Complete(int affectedCount)
    {
        Status = "Completed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        DurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
        AffectedCount = affectedCount;
    }

    /// <summary>
    /// 标记步骤执行失败。记录错误信息及耗时
    /// </summary>
    /// <param name="error">错误消息</param>
    public void Fail(string error)
    {
        Status = "Failed";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        DurationMs = (int)(CompletedAt.Value - StartedAt).TotalMilliseconds;
        ErrorMessage = error;
    }

    /// <summary>
    /// 标记步骤被跳过。记录跳过的原因
    /// </summary>
    /// <param name="reason">跳过原因，如"条件不满足"、"上游步骤失败"</param>
    public void Skip(string reason)
    {
        Status = "Skipped";
        CompletedAt = RBS.Core.Common.ChinaTime.Now;
        Message = reason;
    }
}
