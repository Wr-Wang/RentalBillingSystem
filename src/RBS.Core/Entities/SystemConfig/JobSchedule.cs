namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

/// <summary>
/// 排期配置 — 定义定时任务的执行计划（AuditableEntity）
/// 支持每日/每月两种调度类型，可配置执行时间（时/分）和每月执行日。
/// 通过 TemplateCode 关联任务模板，记录上次执行状态供监控使用
/// </summary>
public class JobSchedule : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 任务名称，标识要执行的具体任务，如 "租金计算"、"利息计算"
    /// </summary>
    public string JobName { get; private set; } = string.Empty;

    /// <summary>
    /// 调度类型：Daily=每日执行, Monthly=每月指定日执行
    /// 默认值为 "Monthly"
    /// </summary>
    public string ScheduleType { get; private set; } = "Monthly";

    /// <summary>
    /// 执行小时（0-23），默认 8 点
    /// </summary>
    public int Hour { get; private set; } = 8;

    /// <summary>
    /// 执行分钟（0-59），默认 0 分
    /// </summary>
    public int Minute { get; private set; } = 0;

    /// <summary>
    /// 每月执行日（1-31），仅 ScheduleType=Monthly 时有效。
    /// 默认值为 1
    /// </summary>
    public int? DayOfMonth { get; private set; }

    /// <summary>
    /// 是否启用，true=启用（调度引擎将按计划执行），false=停用（跳过执行）
    /// 默认值为 true
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 任务描述（可选），说明该任务的功能和用途
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 所属公司标识，每家公司可独立配置任务排期
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 关联的任务模板编码（可选），关联到 JobTemplate 的 Code
    /// </summary>
    public string? TemplateCode { get; private set; }

    /// <summary>
    /// 上次执行时间（可选），由 RecordRun 记录
    /// </summary>
    public DateTime? LastRunAt { get; private set; }

    /// <summary>
    /// 上次执行状态（可选），如 "Completed"、"Failed"
    /// </summary>
    public string? LastRunStatus { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private JobSchedule() { }

    /// <summary>
    /// 创建排期配置实例。Monthly 类型自动设置 DayOfMonth，Daily 类型 DayOfMonth 为 null
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="scheduleType">调度类型（Daily/Monthly）</param>
    /// <param name="hour">执行小时（0-23）</param>
    /// <param name="minute">执行分钟（0-59）</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="dayOfMonth">每月执行日（仅 Monthly 类型有效）</param>
    public JobSchedule(string jobName, string scheduleType, int hour, int minute, Guid companyId, int? dayOfMonth = null)
    {
        JobName = jobName;
        ScheduleType = scheduleType;
        Hour = hour;
        Minute = minute;
        DayOfMonth = (scheduleType == "Monthly") ? (dayOfMonth ?? 1) : null;
        CompanyId = companyId;
    }

    /// <summary>设置任务名称</summary>
    /// <param name="name">任务名称</param>
    public void SetJobName(string name) => JobName = name;

    /// <summary>设置调度计划</summary>
    /// <param name="type">调度类型（Daily/Monthly）</param>
    /// <param name="hour">执行小时（0-23）</param>
    /// <param name="minute">执行分钟（0-59）</param>
    /// <param name="dayOfMonth">每月执行日（仅 Monthly 类型有效）</param>
    public void SetSchedule(string type, int hour, int minute, int? dayOfMonth = null)
    {
        ScheduleType = type;
        Hour = hour;
        Minute = minute;
        DayOfMonth = (type == "Monthly") ? (dayOfMonth ?? 1) : null;
    }

    /// <summary>设置任务描述</summary>
    /// <param name="desc">描述文本</param>
    public void SetDescription(string? desc) => Description = desc;

    /// <summary>启用排期</summary>
    public void Activate() => IsActive = true;

    /// <summary>停用排期</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>设置关联的任务模板编码</summary>
    /// <param name="code">模板编码</param>
    public void SetTemplateCode(string? code) => TemplateCode = code;

    /// <summary>记录一次任务执行结果（更新最后运行时间和状态）</summary>
    /// <param name="status">执行状态，如 "Completed"、"Failed"</param>
    public void RecordRun(string status)
    {
        LastRunAt = ChinaTime.Now;
        LastRunStatus = status;
    }

    /// <summary>
    /// 获取调度描述文字，用于界面展示。
    /// 如 "每天 08:00" 或 "每月25日 08:00"
    /// </summary>
    public string ScheduleDisplay =>
        ScheduleType == "Daily"
            ? $"每天 {Hour:D2}:{Minute:D2}"
            : $"每月{DayOfMonth}日 {Hour:D2}:{Minute:D2}";
}
