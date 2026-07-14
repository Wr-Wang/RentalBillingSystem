namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 排期执行实例 — 每条记录一个具体的执行日期时间（AuditableEntity）
/// 由调度引擎根据 JobSchedule 配置生成，表示某次任务在特定日期/时间的执行计划。
/// 支持日期调整（Adjusted）、手动创建（Custom）和多种状态流转：
/// Pending -> Processing -> Completed/Failed/Skipped/Paused/Cancelled
/// </summary>
public class JobScheduleExecution : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 关联的排期配置标识，指向 JobSchedule
    /// </summary>
    public Guid JobScheduleId { get; private set; }

    /// <summary>
    /// 所属公司标识，用于多租户数据隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 计划执行日期（含时间），标识该执行实例的执行时间点。
    /// 可能因节假日/调休等因素被调整
    /// </summary>
    public DateTime TargetDate { get; private set; }

    /// <summary>
    /// 原始计划日期（可选），记录调整前的原始执行日期。
    /// 当 IsAdjusted=true 时有值，用于审计追溯
    /// </summary>
    public DateTime? OriginalDate { get; private set; }

    /// <summary>
    /// 目标月份，格式 "yyyy-MM"，如 "2026-07"。
    /// 用于按月查询和统计
    /// </summary>
    public string Month { get; private set; } = null!;

    /// <summary>
    /// 执行状态：
    /// Pending=待执行, Processing=执行中, Completed=已完成,
    /// Failed=失败, Skipped=跳过, Paused=暂停, Cancelled=取消
    /// 默认值为 "Pending"
    /// </summary>
    public string Status { get; private set; } = "Pending";

    /// <summary>
    /// 状态原因（可选），记录状态变更的说明，
    /// 如跳过的原因、失败的错误消息、暂停的说明等
    /// </summary>
    public string? Reason { get; private set; }

    /// <summary>
    /// 是否被调整过。true=日期因节假日/调休等原因被调整，
    /// 或者手动修改过执行计划
    /// </summary>
    public bool IsAdjusted { get; private set; }

    /// <summary>
    /// 是否手动创建。true=由管理员手动创建的额外执行实例，
    /// false=由调度引擎自动生成
    /// </summary>
    public bool IsCustom { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private JobScheduleExecution() { }

    /// <summary>
    /// 创建排期执行实例。初始状态为 Pending
    /// </summary>
    /// <param name="jobScheduleId">排期配置标识</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="targetDate">计划执行日期</param>
    /// <param name="originalDate">原始计划日期（可选）</param>
    /// <param name="month">目标月份，格式 "yyyy-MM"</param>
    /// <param name="isCustom">是否手动创建</param>
    public JobScheduleExecution(Guid jobScheduleId, Guid companyId,
        DateTime targetDate, DateTime? originalDate, string month, bool isCustom)
    {
        JobScheduleId = jobScheduleId;
        CompanyId = companyId;
        TargetDate = targetDate;
        OriginalDate = originalDate;
        Month = month;
        IsCustom = isCustom;
        Status = "Pending";
    }

    /// <summary>
    /// 更新执行实例的计划日期、状态和原因，同时标记为已调整
    /// </summary>
    /// <param name="targetDate">新的计划执行日期</param>
    /// <param name="status">新的执行状态</param>
    /// <param name="reason">状态变更原因（可选）</param>
    public void Update(DateTime targetDate, string status, string? reason)
    {
        TargetDate = targetDate;
        Status = status;
        Reason = reason;
        IsAdjusted = true;
    }

    /// <summary>标记为已调整（日期因节假日等原因被修改）</summary>
    public void MarkAdjusted() => IsAdjusted = true;

    /// <summary>标记为执行中（调度引擎开始执行时调用）</summary>
    public void MarkProcessing() => Status = "Processing";

    /// <summary>标记为已完成</summary>
    public void MarkCompleted() { Status = "Completed"; IsAdjusted = true; }

    /// <summary>标记为失败</summary>
    /// <param name="reason">失败原因</param>
    public void MarkFailed(string? reason) { Status = "Failed"; Reason = reason; }

    /// <summary>
    /// 跳过 — 上游失败阻断或管理员手动跳过
    /// </summary>
    /// <param name="reason">跳过原因</param>
    public void MarkSkipped(string? reason) { Status = "Skipped"; Reason = reason; }

    /// <summary>
    /// 暂停 — 管理员手动暂停，调度引擎不会拾取
    /// </summary>
    /// <param name="reason">暂停原因</param>
    public void MarkPaused(string? reason) { Status = "Paused"; Reason = reason; }

    /// <summary>
    /// 取消 — 管理员手动取消，终态不可逆转
    /// </summary>
    /// <param name="reason">取消原因</param>
    public void MarkCancelled(string? reason) { Status = "Cancelled"; Reason = reason; }

    /// <summary>
    /// 重置为待执行状态（从 Skipped/Paused/Failed 恢复），
    /// 允许调度引擎重新拾取该执行实例
    /// </summary>
    /// <param name="reason">重置原因（可选）</param>
    public void ResetToPending(string? reason = null) { Status = "Pending"; Reason = reason; }
}
