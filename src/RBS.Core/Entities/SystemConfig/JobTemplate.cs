namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 任务模板 — 预定义的任务类型模板（AuditableEntity）
/// 用于定义系统中可创建的任务类型及其默认排期参数。
/// 当用户创建新排期时，选择模板可快速填充默认配置。
/// 包含编码、显示名称、图标、分类及默认调度时间等
/// </summary>
public class JobTemplate : AuditableEntity
{
    /// <summary>
    /// 模板编码（唯一），用于程序识别和关联 JobSchedule.TemplateCode，
    /// 如 "RentCalculation"、"InterestCalculation"
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 模板显示名称，如 "租金计算"、"利息计算"，用于界面列表展示
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// 模板短名称，如 "计算租金"、"计算利息"，用于紧凑界面展示
    /// </summary>
    public string ShortName { get; private set; } = string.Empty;

    /// <summary>
    /// 默认调度类型：Daily=每日, Monthly=每月
    /// 默认值为 "Monthly"
    /// </summary>
    public string DefaultScheduleType { get; private set; } = "Monthly";

    /// <summary>
    /// 默认执行小时（0-23），默认 8 点
    /// </summary>
    public int DefaultHour { get; private set; } = 8;

    /// <summary>
    /// 默认执行分钟（0-59），默认 0 分
    /// </summary>
    public int DefaultMinute { get; private set; } = 0;

    /// <summary>
    /// 默认每月执行日（仅 Monthly 类型有效）
    /// </summary>
    public int? DefaultDayOfMonth { get; private set; }

    /// <summary>
    /// 模板描述（可选），说明该任务模板的功能和用途
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 图标样式（可选），用于前端界面展示，如 "el-icon-document"
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// 模板分类，用于前端分组展示，如 "Billing"（计费）、"System"（系统）
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// 排序序号，控制模板在列表中的显示顺序，数值越小越靠前
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 是否启用，true=启用（可选择该模板），false=停用（隐藏）
    /// 默认值为 true
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private JobTemplate() { }

    /// <summary>
    /// 创建任务模板实例
    /// </summary>
    /// <param name="code">模板编码（唯一）</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="shortName">短名称</param>
    /// <param name="scheduleType">默认调度类型（Daily/Monthly）</param>
    /// <param name="hour">默认执行小时</param>
    /// <param name="minute">默认执行分钟</param>
    /// <param name="dayOfMonth">默认每月执行日（可选）</param>
    /// <param name="description">模板描述（可选）</param>
    /// <param name="icon">图标样式（可选）</param>
    /// <param name="category">模板分类</param>
    /// <param name="sortOrder">排序序号</param>
    public JobTemplate(string code, string displayName, string shortName,
        string scheduleType, int hour, int minute, int? dayOfMonth,
        string? description, string? icon, string category, int sortOrder)
    {
        Code = code;
        DisplayName = displayName;
        ShortName = shortName;
        DefaultScheduleType = scheduleType;
        DefaultHour = hour;
        DefaultMinute = minute;
        DefaultDayOfMonth = dayOfMonth;
        Description = description;
        Icon = icon;
        Category = category;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 获取默认调度描述文字，用于界面展示。
    /// 如 "每天 08:00" 或 "每月1日 08:00"
    /// </summary>
    public string DefaultDisplay =>
        DefaultScheduleType == "Daily"
            ? $"每天 {DefaultHour:D2}:{DefaultMinute:D2}"
            : $"每月{DefaultDayOfMonth}日 {DefaultHour:D2}:{DefaultMinute:D2}";
}
